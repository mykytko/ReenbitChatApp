using System.Text.Json.Serialization;

namespace ReenbitChatApp.EFL;

public class User : BaseEntity
{
    public string Login { get; set; }
    public string Password { get; set; }
    
    [JsonIgnore]
    public ICollection<MembersInChat> Chats { get; set; }
    
    [JsonIgnore]
    public ICollection<Message> Messages { get; set; }
    
    [JsonIgnore]
    public ICollection<MessageDeletedForUser> MessagesDeletedForMe { get; set; }
}