using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ReenbitChatApp.EFL;

public class MessageDeletedForUser : BaseEntity
{
    public int MessageId { get; set; }
    public int UserId { get; set; }
    
    [JsonIgnore]
    public Message Message { get; set; }
    
    [JsonIgnore]
    public User User { get; set; }
}