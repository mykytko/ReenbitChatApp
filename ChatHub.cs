using System.Globalization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReenbitChatApp.EFL;

namespace ReenbitChatApp;

/// <summary>
/// Main SignalR hub.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatHub : Hub
{
    private readonly AppDbContext _appDbContext;

    public ChatHub(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    /// <summary>
    /// On connection registers the connected username for later use.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, Context.User!.Identity!.Name!);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Gets surface level descriptions for chats available to the caller user.
    /// </summary>
    public async Task GetChats()
    {
        // Get the user ID
        var userId = _appDbContext.Users.First(u => u.Login == Context.User!.Identity!.Name).Id;
        // Get messages that are locally deleted for the user
        var locallyDeletedMessages = _appDbContext.MessageDeletedForUsers
            .Where(mdfu => mdfu.UserId == userId)
            .Select(mdfu => mdfu.MessageId)
            .AsEnumerable();
        
        // Get IDs of chats that the user is a member of
        var chatIds = _appDbContext.MembersInChats
            .Where(mic => mic.UserId == userId)
            .Select(mic => mic.ChatId)
            .ToList();
        
        // Get a map of all {message -> user who sent the message} IDs
        var map = _appDbContext.Messages.ToDictionary(m => m.Id, m => m.UserId);
        var result = chatIds.Select(id =>
        {
            var message = _appDbContext.Chats
                .Include(c => c.Messages)
                .ThenInclude(m => m.User)
                .First(c => c.Id == id)
                .Messages
                .Where(m =>
                {
                    // Filter out the messages that are not supposed to be seen by the user
                    var isNotDeleted = locallyDeletedMessages.All(deletedMessageId => deletedMessageId != m.Id);
                    var isPersonallyValid = true;
                    if (m.ReplyTo != -1 && m.ReplyIsPersonal)
                    {
                        isPersonallyValid = userId == map[m.Id] || userId == map[m.ReplyTo];
                    }
                    return isNotDeleted && isPersonallyValid;
                })
                .MaxBy(m => m.DateTime)!;

            // Prepare data for the response
            var chatName = _appDbContext.Chats.First(c => c.Id == id).ChatName;
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

        // Send the response
        await Clients.Client(Context.ConnectionId).SendAsync("GetChats", result);
    }

    /// <summary>
    /// Gets 20 newest messages skipping the last <paramref name="skip"/> messages
    /// from the chat <paramref name="chatName"/>.
    /// </summary>
    /// <param name="skip">The amount of latest messages to skip.</param>
    /// <param name="chatName">The name of the chat from which to take the messages.</param>
    public async Task GetMessages(int skip, string chatName)
    {
        // Filter messages in a similar way to the one used in GetChats method
        var userId = _appDbContext.Users.FirstOrDefault(u => u.Login == Context.User!.Identity!.Name)?.Id;
        if (!userId.HasValue)
        {
            return;
        }
        
        var locallyDeletedMessages = _appDbContext.MessageDeletedForUsers
            .Where(mdfu => mdfu.UserId == userId)
            .Select(mdfu => mdfu.MessageId)
            .AsEnumerable();
        var map = _appDbContext.Messages
            .ToDictionary(m => m.Id, m => m.UserId);
        await Clients.Client(Context.ConnectionId).SendAsync("GetMessages", 
            chatName,
            _appDbContext.Messages
                .Include(m => m.Chat)
                .Include(m => m.User)
                .Where(m => m.Chat.ChatName == chatName && !locallyDeletedMessages.Contains(m.Id))
                .AsEnumerable()
                .Where(m => m.UserId == userId || !m.ReplyIsPersonal || map[m.ReplyTo] == userId)
                .OrderByDescending(m => m.DateTime)
                .Skip(skip)
                .Take(20)
                .Select(m => new
                {
                    id = m.Id,
                    username = m.User.Login,
                    text = m.Text,
                    date = m.DateTime.ToString(CultureInfo.InvariantCulture) + " UTC",
                    replyTo = m.ReplyTo,
                    replyIsPersonal = m.ReplyIsPersonal
                })
        );
    }

    /// <summary>
    /// Deletes a message by ID from database and broadcasts the change to all users.
    /// </summary>
    /// <param name="messageId">ID of the message to delete.</param>
    public async Task BroadcastDelete(int messageId)
    {
        // Get the message
        var message = _appDbContext.Messages
            .Include(m => m.Chat)
            .Include(m => m.User)
            .FirstOrDefault(m => m.Id == messageId);
        if (message == null)
        {
            return;
        }
        
        // Check if the caller owns the message
        if (message.User.Login != Context.User!.Identity!.Name)
        {
            return;
        }

        // Delete from database
        var chatName = message.Chat.ChatName;
        _appDbContext.Messages.Remove(message);
        await _appDbContext.SaveChangesAsync();
        
        // Broadcast the change
        await Clients.All.SendAsync("BroadcastDelete", chatName, messageId);
    }

    /// <summary>
    /// Edits the specified message in database and broadcasts the change.
    /// </summary>
    /// <param name="messageId">The ID of the message to edit.</param>
    /// <param name="messageText">The new text of the message.</param>
    public async Task BroadcastEdit(int messageId, string messageText)
    {
        // Don't allow whitespace messages
        if (string.IsNullOrWhiteSpace(messageText))
        {
            return;
        }
        
        // The rest is analogous to BroadcastDelete
        var message = _appDbContext.Messages
            .Include(m => m.Chat)
            .Include(m => m.User)
            .FirstOrDefault(m => m.Id == messageId);
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

    /// <summary>
    /// Writes a new message to database and broadcasts it to all users.
    /// </summary>
    /// <param name="chatName">The name of the chat where the message was sent.</param>
    /// <param name="messageText">The text of the message.</param>
    /// <param name="replyTo">If this message is a reply, this specified the ID of the message
    /// to which it is a reply. If it is not a reply, pass -1.</param>
    /// <param name="replyIsPersonal">If this message is a reply, this specifies if it's personal or
    /// visible to all. If it is not a reply, this parameter doesn't affect anything.</param>
    public async Task BroadcastMessage(string chatName, string messageText, int replyTo, bool replyIsPersonal)
    {
        // Don't allow whitespace messages
        if (string.IsNullOrWhiteSpace(messageText))
        {
            return;
        }
        
        var chatId = _appDbContext.Chats.FirstOrDefault(c => c.ChatName == chatName)?.Id;
        if (!chatId.HasValue)
        {
            return;
        }

        var login = Context.User?.Identity?.Name;
        if (login == null)
        {
            return;
        }

        var userId = _appDbContext.Users.FirstOrDefault(u => u.Login == login)?.Id;
        if (!userId.HasValue)
        {
            return;
        }
        
        // Check if the user is a member of the specified chat.
        if (!_appDbContext.MembersInChats
            .Where(mic => mic.ChatId == chatId)
            .Any(mic => mic.UserId == userId))
        {
            return;
        }

        // Add the message to the database.
        var now = DateTime.UtcNow;
        var message = _appDbContext.Messages.Add(new Message 
        {
            ChatId = chatId.Value,
            UserId = userId.Value,
            Text = messageText,
            DateTime = now,
            ReplyTo = replyTo,
            ReplyIsPersonal = replyIsPersonal
        });
        await _appDbContext.SaveChangesAsync();

        // Create the message to broadcast
        var broadcastedMessage = new
        {
            id = message.Entity.Id,
            username = login,
            text = messageText,
            date = now.ToString(CultureInfo.InvariantCulture) + " UTC",
            replyTo,
            replyIsPersonal
        };
        
        // If it's a personal reply, broadcast only to the caller and to the target user.
        if (replyTo != -1 && replyIsPersonal)
        {
            var replyMessage = _appDbContext.Messages
                .Include(m => m.User)
                .FirstOrDefault(m => m.Id == replyTo);
            if (replyMessage == null)
            {
                return;
            }
            
            await Clients.Group(replyMessage.User.Login)
                .SendAsync("BroadcastMessage", chatName, broadcastedMessage);
            await Clients.Client(Context.ConnectionId)
                .SendAsync("BroadcastMessage", chatName, broadcastedMessage);
        }
        else
        {
            await Clients.All.SendAsync("BroadcastMessage", chatName, broadcastedMessage);
        }
    }

    /// <summary>
    /// Delete the message locally for the caller.
    /// </summary>
    /// <param name="id">The ID of the message to delete.</param>
    public async Task DeleteForMe(int id)
    {
        var message = _appDbContext.Messages
            .Include(m => m.Chat)
            .FirstOrDefault(m => m.Id == id);
        if (message == null)
        {
            return;
        }
        
        var userId = _appDbContext.Users.First(u => u.Login == Context.User!.Identity!.Name).Id;
        _appDbContext.MessageDeletedForUsers.Add(new MessageDeletedForUser
        {
            MessageId = id,
            UserId = userId
        });
        await _appDbContext.SaveChangesAsync();
        
        var chatName = message.Chat.ChatName;
        await Clients.Client(Context.ConnectionId).SendAsync("BroadcastDelete", chatName, id);
    }
}