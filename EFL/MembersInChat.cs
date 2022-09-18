using System.Text.Json.Serialization;

namespace ReenbitChatApp.EFL;

public class MembersInChat : BaseEntity
{
    public int UserId { get; set; }
    public int ChatId { get; set; }
    
    [JsonIgnore]
    public User User { get; set; }
    
    
    [JsonIgnore]
    public Chat Chat { get; set; }
}