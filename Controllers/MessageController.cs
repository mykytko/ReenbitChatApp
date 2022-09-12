using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReenbitChatApp.EFL;

namespace ReenbitChatApp.Controllers;

public class MessageController : ControllerBase
{
    private readonly AppDbContext _appDbContext;

    public MessageController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    [HttpGet, Route(""), Route("message"), Route("message/get")]
    public IActionResult Get()
    {
        return Ok(_appDbContext.Chats.Include(c => c.Messages).First()
            .Messages.AsEnumerable());
    }
}