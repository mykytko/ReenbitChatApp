using System.ComponentModel.DataAnnotations;
using ReenbitChatApp.EFL;

namespace ReenbitChat.EFL;

public class Chat
{
    [Key]
    public int ChatId { get; set; }
    public ICollection<User> Members { get; set; }
    public ICollection<Message> Messages { get; set; }
}