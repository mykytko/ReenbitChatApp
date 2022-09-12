using Microsoft.AspNetCore.Mvc;
using ReenbitChat.EFL;

namespace ReenbitChat.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly AppDbContext _appDbContext;

    public MessageController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    [HttpGet]
    public IEnumerable<Message> Get()
    {
        return _appDbContext.Chats.First().Messages.Select(m => _appDbContext.Messages.Find(m.Id)!);
    }
}