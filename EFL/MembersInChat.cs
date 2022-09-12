using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReenbitChat.EFL;

namespace ReenbitChatApp.EFL;

public class MembersInChat
{
    [Key]
    public int MembersInChatId { get; set; }
    public int UserId { get; set; }
    public int ChatId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }
    
    [ForeignKey("ChatId")]
    public Chat Chat { get; set; }
}