using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReenbitChatApp.EFL;

namespace ReenbitChatApp;

public static class DataProvider
{
    public static void Provide(ModelBuilder modelBuilder)
    {
        var users = new List<User>
        {
            new() {Id = 1, Login = "mykytko", Password = "password1"},
            new() {Id = 2, Login = "lina", Password = "password2"},
            new() {Id = 3, Login = "vlad", Password = "password3"},
            new() {Id = 4, Login = "andriy", Password = "password4"}
        };
        var hasher = new PasswordHasher<User>();
        users.ForEach(u => u.Password = hasher.HashPassword(u, u.Password));
        modelBuilder.Entity<User>().HasData(users);
        
        modelBuilder.Entity<Message>().HasData(
            new Message
            {
                Id = 1,
                UserId = 1,
                Text = "bonjour",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(24),
                ChatId = 1,
                ReplyTo = -1,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 2,
                UserId = 2,
                Text = "yeah, hello",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(23),
                ChatId = 1,
                ReplyTo = 1,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 3,
                UserId = 3,
                Text = "how are you guys doing?",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(22),
                ChatId = 1,
                ReplyTo = -1,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 4,
                UserId = 1,
                Text = "pretty well",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(21),
                ChatId = 1,
                ReplyTo = 3,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 5,
                UserId = 2,
                Text = "same",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(20),
                ChatId = 1,
                ReplyTo = 4,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 14,
                UserId = 2,
                Text = "don't you think vlad is annoying?",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(20) + TimeSpan.FromMinutes(10),
                ChatId = 1,
                ReplyTo = 4,
                ReplyIsPersonal = true
            },
            new Message
            {
                Id = 15,
                UserId = 1,
                Text = "i do",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(20) + TimeSpan.FromMinutes(20),
                ChatId = 1,
                ReplyTo = 14,
                ReplyIsPersonal = true
            },
            new Message
            {
                Id = 6,
                UserId = 3,
                Text = "great!",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(19),
                ChatId = 1,
                ReplyTo = -1,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 7,
                UserId = 3,
                Text = "can you help me?",
                DateTime = DateTime.UtcNow - TimeSpan.FromMinutes(6),
                ChatId = 2,
                ReplyTo = -1,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 8,
                UserId = 4,
                Text = "sorry, i'm busy right now. maybe later",
                DateTime = DateTime.UtcNow - TimeSpan.FromMinutes(3),
                ChatId = 2,
                ReplyTo = -1,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 9,
                UserId = 3,
                Text = "it's urgent ;(",
                DateTime = DateTime.UtcNow - TimeSpan.FromMinutes(3),
                ChatId = 2,
                ReplyTo = -1,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 10,
                UserId = 4,
                Text = "fine, what's the problem?",
                DateTime = DateTime.UtcNow - TimeSpan.FromMinutes(1),
                ChatId = 2,
                ReplyTo = -1,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 11,
                UserId = 4,
                Text = "Vlad is annoying.",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(18),
                ChatId = 3,
                ReplyTo = -1,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 12,
                UserId = 2,
                Text = "+1",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(17),
                ChatId = 3,
                ReplyTo = 11,
                ReplyIsPersonal = false
            },
            new Message
            {
                Id = 13,
                UserId = 1,
                Text = "Finally we have a calm chat without him",
                DateTime = DateTime.UtcNow - TimeSpan.FromHours(16),
                ChatId = 3,
                ReplyTo = -1,
                ReplyIsPersonal = false
            });
        
        modelBuilder.Entity<MembersInChat>().HasData(
            new MembersInChat
            {
                Id = 1,
                ChatId = 1,
                UserId = 1
            },
            new MembersInChat
            {
                Id = 2,
                ChatId = 1,
                UserId = 2
            },
            new MembersInChat
            {
                Id = 3,
                ChatId = 1,
                UserId = 3
            },
            new MembersInChat
            {
                Id = 4,
                ChatId = 1,
                UserId = 4
            },
            new MembersInChat
            {
                Id = 5,
                ChatId = 2,
                UserId = 3
            },
            new MembersInChat
            {
                Id = 6,
                ChatId = 2,
                UserId = 4
            },
            new MembersInChat
            {
                Id = 7,
                ChatId = 3,
                UserId = 1
            },
            new MembersInChat
            {
                Id = 8,
                ChatId = 3,
                UserId = 2
            },
            new MembersInChat
            {
                Id = 9,
                ChatId = 3,
                UserId = 4
            });

        modelBuilder.Entity<Chat>().HasData(
            new Chat {Id = 1, ChatName = "group chat"},
            new Chat {Id = 2, ChatName = "andriy;vlad"},
            new Chat {Id = 3, ChatName = "group chat without vlad"});
    }
}