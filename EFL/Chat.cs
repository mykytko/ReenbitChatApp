using System.ComponentModel.DataAnnotations;

namespace ReenbitChatApp.EFL;

public class Chat
{
    [Key]
    public int ChatId { get; set; }
    public string ChatName { get; set; }
    public ICollection<User> Members { get; set; }
    public ICollection<Message> Messages { get; set; }
}