using Microsoft.EntityFrameworkCore;
using ReenbitChat.EFL;

namespace ReenbitChatApp.EFL;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Chat> Chats { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}