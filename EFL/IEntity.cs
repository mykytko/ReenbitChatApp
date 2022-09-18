using System.ComponentModel.DataAnnotations;

namespace ReenbitChatApp.EFL;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
}