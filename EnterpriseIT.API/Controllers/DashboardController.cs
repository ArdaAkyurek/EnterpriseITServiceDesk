using System.Security.Claims;
using EnterpriseIT.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Technician,Employee")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DashboardController(ITicketRepository tickets) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await tickets.GetDashboardAsync(userId, User.FindFirstValue(ClaimTypes.Role)!));
    }
}
