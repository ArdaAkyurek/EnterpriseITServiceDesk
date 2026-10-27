using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using EnterpriseIT.Application.DTOs.Notification;
using EnterpriseIT.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Technician,Employee")]
[Route("api/[controller]")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class NotificationController(INotificationRepository repository, IMapper mapper) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([Range(1, 1000000)] int page = 1)
    {
        var items = await repository.GetAllWithDetailsAsync(UserId, page);
        return Ok(new NotificationPageDto
        {
            Items = mapper.Map<List<NotificationDto>>(items.Take(20)), Page = page,
            HasMore = items.Count > 20, UnreadCount = await repository.GetUnreadCountAsync(UserId)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await repository.GetByIdWithDetailsAsync(id, UserId);
        return item == null ? NotFound() : Ok(mapper.Map<NotificationDto>(item));
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var item = await repository.GetByIdWithDetailsAsync(id, UserId);
        if (item == null) return NotFound();
        if (!item.IsRead)
        {
            item.IsRead = true;
            item.UpdatedAt = DateTime.UtcNow;
            await repository.SaveChangesAsync();
        }
        return NoContent();
    }
}
