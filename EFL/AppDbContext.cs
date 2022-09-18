using Microsoft.EntityFrameworkCore;


namespace ReenbitChatApp.EFL;

/// <summary>
/// A class for Application database context. Inherits DbContext. 
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Chat> Chats { get; set; }
    /// <summary>
    /// A table connecting chats and members that are in it.
    /// </summary>
    public DbSet<MembersInChat> MembersInChats { get; set; }
    /// <summary>
    /// A table describing which messages are locally deleted by which users.
    /// </summary>
    public DbSet<MessageDeletedForUser> MessageDeletedForUsers { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Azure database parameters
        modelBuilder.HasDatabaseMaxSize("100 MB");
        modelBuilder.HasServiceTier("Basic");
        modelBuilder.HasPerformanceLevel("Basic");

        // Fluent API description of table relations
        modelBuilder.Entity<Chat>()
            .HasMany(c => c.Members)
            .WithOne(c => c.Chat);
        modelBuilder.Entity<Chat>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.User)
            .WithMany(u => u.Messages);
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Messages);
        modelBuilder.Entity<Message>()
            .HasMany(m => m.DeletedForUsers)
            .WithOne(mdfu => mdfu.Message);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Chats)
            .WithOne(c => c.User);
        modelBuilder.Entity<User>()
            .HasMany(u => u.Messages)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId);
        modelBuilder.Entity<User>()
            .HasMany(u => u.MessagesDeletedForMe)
            .WithOne(mdfu => mdfu.User)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MembersInChat>()
            .HasOne(mic => mic.Chat);
        modelBuilder.Entity<MembersInChat>()
            .HasOne(mic => mic.User);

        modelBuilder.Entity<MessageDeletedForUser>()
            .HasOne(mdfu => mdfu.Message);
        modelBuilder.Entity<MessageDeletedForUser>()
            .HasOne(mdfu => mdfu.User)
            .WithMany(u => u.MessagesDeletedForMe);

        // Provide a description of default data for the database
        // This will be used only on EnsureCreated() call
        DataProvider.Provide(modelBuilder);
    }
}