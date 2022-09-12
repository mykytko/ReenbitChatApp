using Microsoft.EntityFrameworkCore;

namespace ReenbitChat.EFL;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Chat> Chats { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}