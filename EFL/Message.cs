using System.Text.Json.Serialization;

namespace ReenbitChatApp.EFL;

public class Message : BaseEntity
{
    public int UserId { get; set; }
    public int ReplyTo { get; set; }
    public bool ReplyIsPersonal { get; set; }
    public int ChatId { get; set; }
    public string Text { get; set; }
    public DateTime DateTime { get; set; }


    [JsonIgnore]
    public User User { get; set; }
    
    
    [JsonIgnore]
    public Chat Chat { get; set; }
    
    [JsonIgnore]
    public ICollection<MessageDeletedForUser> DeletedForUsers { get; set; }
}