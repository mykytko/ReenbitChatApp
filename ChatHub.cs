using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReenbitChatApp.EFL;

namespace ReenbitChatApp;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatHub : Hub
{
    private readonly AppDbContext _appDbContext;

    public ChatHub(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task GetChats()
    {
        var userId = _appDbContext.Users.First(u => u.Login == Context.User!.Identity!.Name).UserId;
        var chatIds = _appDbContext.MembersInChats
            .Where(mic => mic.UserId == userId)
            .Select(mic => mic.ChatId)
            .ToList();

        var result = chatIds.Select(id =>
        {
            var message = _appDbContext.Chats
                .Include(c => c.Messages)
                .ThenInclude(m => m.User)
                .First(c => c.ChatId == id)
                .Messages
                .MaxBy(m => m.DateTime)!;

            var chatName = _appDbContext.Chats.First(c => c.ChatId == id).ChatName;
            var lastMessageSender = message.User.Login;
            var lastMessageText = message.Text;
            var isPersonal = _appDbContext.MembersInChats.Count(mic => mic.ChatId == id) == 2;

            return new
            {
                chatName,
                lastMessageSender,
                lastMessageText,
                isPersonal
            };
        }).ToList();

        await Clients.Client(Context.ConnectionId).SendAsync("GetChats", result);
    }

    public async Task GetMessages(int skip, string chatName) =>
        await Clients.Client(Context.ConnectionId).SendAsync("GetMessages", 
                chatName,
                _appDbContext.Messages
                    .Include(m => m.Chat)
                    .Where(m => m.Chat.ChatName == chatName)
                    .Include(m => m.User)
                    .OrderByDescending(m => m.DateTime)
                    .Skip(skip)
                    .Take(20)
                    .Select(m => new
                    {
                        id = m.MessageId,
                        username = m.User.Login,
                        text = m.Text,
                        date = m.DateTime.ToLocalTime().ToString(CultureInfo.CurrentCulture),
                        replyTo = m.ReplyTo
                    })
                    .ToList()
            );

    public async Task BroadcastDelete(int messageId)
    {
        var message = _appDbContext.Messages
            .Include(m => m.Chat)
            .Include(m => m.User)
            .FirstOrDefault(m => m.MessageId == messageId);
        if (message == null)
        {
            return;
        }
        
        if (message.User.Login != Context.User!.Identity!.Name)
        {
            return;
        }

        var chatName = message.Chat.ChatName;
        _appDbContext.Messages.Remove(message);
        await _appDbContext.SaveChangesAsync();
        await Clients.All.SendAsync("BroadcastDelete", chatName, messageId);
    }

    public async Task BroadcastEdit(int messageId, string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
        {
            return;
        }
        
        var message = _appDbContext.Messages
            .Include(m => m.Chat)
            .Include(m => m.User)
            .FirstOrDefault(m => m.MessageId == messageId);
        if (message == null)
        {
            return;
        }

        if (message.User.Login != Context.User!.Identity!.Name)
        {
            return;
        }
        
        message.Text = messageText;
        _appDbContext.Update(message);
        await _appDbContext.SaveChangesAsync();
        
        await Clients.All.SendAsync("BroadcastEdit", 
            message.Chat.ChatName, messageId, messageText);
    }

    public async Task BroadcastMessage(string chatName, string messageText, int replyTo)
    {
        if (string.IsNullOrWhiteSpace(messageText))
        {
            return;
        }
        
        var chat = _appDbContext.Chats
            .Include(c => c.Members)
            .FirstOrDefault(c => c.ChatName == chatName);
        if (chat == null)
        {
            return;
        }

        var login = Context.User?.Identity?.Name;
        if (login == null)
        {
            return;
        }

        var user = _appDbContext.Users.FirstOrDefault(u => u.Login == login);
        if (user == null)
        {
            return;
        }

        if (!chat.Members.Contains(user))
        {
            return;
        }

        var now = DateTime.Now;
        var message = _appDbContext.Messages.Add(new Message 
        {
            ChatId = chat.ChatId,
            UserId = user.UserId,
            Text = messageText,
            DateTime = now,
            ReplyTo = replyTo
        });
        await _appDbContext.SaveChangesAsync();

        await Clients.All.SendAsync("BroadcastMessage", chatName, new 
        {
            id = message.Entity.MessageId,
            username = login,
            text = messageText,
            date = now.ToLocalTime().ToString(CultureInfo.CurrentCulture),
            replyTo
        });
    }
}