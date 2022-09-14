using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReenbitChat.EFL;
using ReenbitChatApp.EFL;

namespace ReenbitChatApp;

public class ChatHub : Hub
{
    private readonly AppDbContext _appDbContext;

    public ChatHub(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task BroadcastMessages(string connectionId, int skip, string chatName) =>
        await Clients.Client(connectionId).SendAsync("BroadcastMessages",
            JsonSerializer.Serialize(
                _appDbContext.Messages
                .Include(m => m.Chat)
                .Where(m => m.Chat.ChatName == chatName)
                .Include(m => m.User)
                .OrderBy(m => m.DateTime)
                .Skip(skip)
                .Take(20)
                .Select(m => new
                {
                    username = m.User.Login,
                    text = m.Text,
                    date = m.DateTime.ToLocalTime().ToString(CultureInfo.CurrentCulture)
                })
                .AsEnumerable()
            ));

    public async Task BroadcastMessage(string chatName, string username, string messageText)
    {
        var chat = _appDbContext.Chats.FirstOrDefault(c => c.ChatName == chatName);
        if (chat == null)
        {
            return;
        }

        var now = DateTime.Now;
        _appDbContext.Messages.Add(new Message
        {
            ChatId = chat.ChatId,
            Text = messageText,
            DateTime = now
        });

        await Clients.All.SendAsync("BroadcastMessage", JsonSerializer.Serialize(
            new
            {
                username,
                text = messageText,
                date = now.ToLocalTime().ToString(CultureInfo.CurrentCulture)
            }
        ));
    }
        
}