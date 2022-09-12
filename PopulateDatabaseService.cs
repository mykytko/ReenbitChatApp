using ReenbitChat.EFL;

namespace ReenbitChat;

public static class PopulateDatabaseService
{
    public static void Populate(AppDbContext dbContext)
    {
        if (dbContext.Users.FirstOrDefault() != default) return;
        var users = new List<User>
        {
            new()
            {
                Id = 0,
                Login = "mykytko",
                Password = "pwd0"
            },
            new()
            {
                Id = 1,
                Login = "lina",
                Password = "pwd1"
            },
            new()
            {
                Id = 2,
                Login = "vlad",
                Password = "pwd2"
            },
            new()
            {
                Id = 3,
                Login = "andriy",
                Password = "pwd3"
            }
        };

        var messages = new List<Message>
        {
            new()
            {
                Id = 0,
                User = users[0],
                Text = "bomjour",
                DateTime = DateTime.Now - TimeSpan.FromHours(24)
            },
            new()
            {
                Id = 1,
                User = users[1],
                Text = "yeah, hello",
                DateTime = DateTime.Now - TimeSpan.FromHours(23)
            },
            new()
            {
                Id = 2,
                User = users[2],
                Text = "how are you guys doing?",
                DateTime = DateTime.Now - TimeSpan.FromHours(22)
            },
            new()
            {
                Id = 3,
                User = users[0],
                Text = "pretty well",
                DateTime = DateTime.Now - TimeSpan.FromHours(21)
            },
            new()
            {
                Id = 4,
                User = users[1],
                Text = "same",
                DateTime = DateTime.Now - TimeSpan.FromHours(20)
            },
            new()
            {
                Id = 5,
                User = users[2],
                Text = "great!",
                DateTime = DateTime.Now - TimeSpan.FromHours(19)
            },
            new()
            {
                Id = 6,
                User = users[3],
                Text = "can you help me?",
                DateTime = DateTime.Now - TimeSpan.FromMinutes(6)
            },
            new()
            {
                Id = 7,
                User = users[2],
                Text = "sorry, i'm busy right now. maybe later",
                DateTime = DateTime.Now - TimeSpan.FromMinutes(3)
            },
            new()
            {
                Id = 8,
                User = users[3],
                Text = "it's urgent ;(",
                DateTime = DateTime.Now - TimeSpan.FromMinutes(3)
            },
            new()
            {
                Id = 9,
                User = users[2],
                Text = "fine, what's the problem?",
                DateTime = DateTime.Now - TimeSpan.FromMinutes(1)
            }
        };

        var chats = new List<Chat>
        {
            new()
            {
                Id = 0,
                Members = new List<User> {users[0], users[1], users[2], users[3]},
                Messages = new List<Message> {messages[0], messages[1], messages[2], 
                    messages[3], messages[4], messages[5]}
            },
            new()
            {
                Id = 1,
                Members = new List<User> {users[2], users[3]},
                Messages = new List<Message> {messages[6], messages[7], messages[8], messages[9]}
            }
        };
        
        // The database is empty, populate it
        dbContext.Users.AddRange(users);
        dbContext.Messages.AddRange(messages);
        dbContext.Chats.AddRange(chats);
    }
}