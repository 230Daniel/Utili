using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Utili.Backend.Models;
using Utili.Database;

namespace Utili.Backend.Controllers;

[Route("message-logs/{Id}")]
public class MessageLogsBulkController : Controller
{
    private readonly IMapper _mapper;
    private readonly DatabaseContext _dbContext;

    public MessageLogsBulkController(IMapper mapper, DatabaseContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync([Required] string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return NotFound();

        var entry = await _dbContext.MessageLogsBulkDeletedMessages
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == guid);

        return entry is null
            ? NotFound()
            : Json(_mapper.Map<MessageLogsBulkDeletedMessagesModel>(entry));
    }
}
