using Microsoft.EntityFrameworkCore;
using ReenbitChatApp.EFL;

namespace ReenbitChatApp;

public static class DbInitializer
{
    public static void Initialize(string connString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connString).Options;
        var context = new AppDbContext(options);
        Console.WriteLine("context initialized");
        context.Database.EnsureDeleted();
        Console.WriteLine("db deleted");
        context.Database.EnsureCreated();
        Console.WriteLine("db created");
        context.Dispose();
        Console.WriteLine("context disposed");
    }
}