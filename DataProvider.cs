using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReenbitChat.EFL;
using ReenbitChatApp.EFL;

namespace ReenbitChatApp;

public static class DataProvider
{
    public static void Provide(ModelBuilder modelBuilder)
    {
        var hasher = new PasswordHasher<User>();
        var users = new List<User>
        {
            new() {UserId = 1, Login = "mykytko", Password = "password1"},
            new() {UserId = 2, Login = "lina", Password = "password2"},
            new() {UserId = 3, Login = "vlad", Password = "password3"},
            new() {UserId = 4, Login = "andriy", Password = "password4"}
        };
        users.ForEach(u => u.Password = hasher.HashPassword(u, u.Password));
        modelBuilder.Entity<User>().HasData(users);
        
        modelBuilder.Entity<Message>().HasData(
            new Message
            {
                MessageId = 1,
                UserId = 1,
                Text = "bomjour",
                DateTime = DateTime.Now - TimeSpan.FromHours(24),
                ChatId = 1
            },
            new 
            {
                MessageId = 2,
                UserId = 2,
                Text = "yeah, hello",
                DateTime = DateTime.Now - TimeSpan.FromHours(23),
                ChatId = 1
            },
            new
            {
                MessageId = 3,
                UserId = 3,
                Text = "how are you guys doing?",
                DateTime = DateTime.Now - TimeSpan.FromHours(22),
                ChatId = 1
            },
            new
            {
                MessageId = 4,
                UserId = 1,
                Text = "pretty well",
                DateTime = DateTime.Now - TimeSpan.FromHours(21),
                ChatId = 1
            },
            new
            {
                MessageId = 5,
                UserId = 2,
                Text = "same",
                DateTime = DateTime.Now - TimeSpan.FromHours(20),
                ChatId = 1
            },
            new
            {
                MessageId = 6,
                UserId = 3,
                Text = "great!",
                DateTime = DateTime.Now - TimeSpan.FromHours(19),
                ChatId = 1
            },
            new
            {
                MessageId = 7,
                UserId = 3,
                Text = "can you help me?",
                DateTime = DateTime.Now - TimeSpan.FromMinutes(6),
                ChatId = 2
            },
            new
            {
                MessageId = 8,
                UserId = 4,
                Text = "sorry, i'm busy right now. maybe later",
                DateTime = DateTime.Now - TimeSpan.FromMinutes(3),
                ChatId = 2
            },
            new
            {
                MessageId = 9,
                UserId = 3,
                Text = "it's urgent ;(",
                DateTime = DateTime.Now - TimeSpan.FromMinutes(3),
                ChatId = 2
            },
            new
            {
                MessageId = 10,
                UserId = 4,
                Text = "fine, what's the problem?",
                DateTime = DateTime.Now - TimeSpan.FromMinutes(1),
                ChatId = 2
            }
            );
        
        modelBuilder.Entity<MembersInChat>().HasData(
            new MembersInChat
            {
                MembersInChatId = 1,
                ChatId = 1,
                UserId = 1
            },
            new MembersInChat
            {
                MembersInChatId = 2,
                ChatId = 1,
                UserId = 2
            },
            new MembersInChat
            {
                MembersInChatId = 3,
                ChatId = 1,
                UserId = 3
            },
            new MembersInChat
            {
                MembersInChatId = 4,
                ChatId = 1,
                UserId = 4
            },
            new MembersInChat
            {
                MembersInChatId = 5,
                ChatId = 2,
                UserId = 3
            },
            new MembersInChat
            {
                MembersInChatId = 6,
                ChatId = 2,
                UserId = 4
            });

        modelBuilder.Entity<Chat>().HasData(new Chat {ChatId = 1}, new Chat {ChatId = 2});
    }
}