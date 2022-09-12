using System.ComponentModel.DataAnnotations;
using ReenbitChat.EFL;

namespace ReenbitChatApp.EFL;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    
    public ICollection<Chat> Chats { get; set; }
    public ICollection<Message> Messages { get; set; }
}