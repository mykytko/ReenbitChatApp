using System.Text.Json.Serialization;

namespace ReenbitChatApp.EFL;

public class Chat : BaseEntity
{
    public string ChatName { get; set; }
    
    [JsonIgnore]
    public ICollection<MembersInChat> Members { get; set; }
    
    [JsonIgnore]
    public ICollection<Message> Messages { get; set; }
}