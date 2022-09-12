using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ReenbitChatApp.EFL;

namespace ReenbitChat.EFL;

public class Message
{
    [Key]
    public int MessageId { get; set; }
    public int UserId { get; set; }
    
    [ForeignKey("UserId"), JsonIgnore]
    public User User { get; set; }
    
    public string Text { get; set; }
    public DateTime DateTime { get; set; }
    
    public int ChatId { get; set; }
    
    [ForeignKey("ChatId"), JsonIgnore]
    public Chat Chat { get; set; }
}