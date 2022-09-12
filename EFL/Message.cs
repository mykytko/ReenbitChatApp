using ReenbitChatApp.EFL;

namespace ReenbitChat.EFL;

public class Message
{
    public int Id { get; set; }
    public User User { get; set; }
    public string Text { get; set; }
    public DateTime DateTime { get; set; }
}