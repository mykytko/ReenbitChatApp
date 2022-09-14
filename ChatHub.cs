using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReenbitChatApp.EFL;

namespace ReenbitChatApp;

public class ChatHub : Hub
{
    private readonly AppDbContext _appDbContext;

    public ChatHub(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task BroadcastMessages(string connectionId, int skip, string chatName)
    {
        var messages = _appDbContext.Messages
            .Include(m => m.Chat)
            .Where(m => m.Chat.ChatName == chatName)
            .Include(m => m.User)
            .OrderByDescending(m => m.DateTime)
            .Skip(skip)
            .Take(20)
            .Select(m => new
            {
                username = m.User.Login,
                text = m.Text,
                date = m.DateTime
            }).AsEnumerable();

        await Clients.Client(connectionId).SendAsync("BroadcastMessages",
            JsonSerializer.Serialize(messages));
    }
}