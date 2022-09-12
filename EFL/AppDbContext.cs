using Microsoft.EntityFrameworkCore;
using ReenbitChat.EFL;


namespace ReenbitChatApp.EFL;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<MembersInChat> MembersInChats { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDatabaseMaxSize("100 MB");
        modelBuilder.HasServiceTier("Basic");
        modelBuilder.HasPerformanceLevel("Basic");
        
        modelBuilder.Entity<User>()
            .HasMany(u => u.Chats)
            .WithMany(c => c.Members);
        modelBuilder.Entity<User>()
            .HasMany(u => u.Messages)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId);
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId);
        modelBuilder.Entity<MembersInChat>()
            .HasOne(mic => mic.Chat);
        modelBuilder.Entity<MembersInChat>()
            .HasOne(mic => mic.User);

        DataProvider.Provide(modelBuilder);
    }
}