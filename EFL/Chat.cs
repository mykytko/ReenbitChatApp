using ReenbitChatApp.EFL;

namespace ReenbitChat.EFL;

public class Chat
{
    public int Id { get; set; }
    public ICollection<User> Members { get; set; }
    public ICollection<Message> Messages { get; set; }
}