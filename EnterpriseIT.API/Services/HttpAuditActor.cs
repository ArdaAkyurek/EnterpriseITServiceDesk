using System.Security.Claims;
using EnterpriseIT.Application.Interfaces;

namespace EnterpriseIT.API.Services;

public sealed class HttpAuditActor(IHttpContextAccessor accessor) : IAuditActor
{
    public int? UserId => accessor.HttpContext?.User.Identity?.IsAuthenticated == true &&
        int.TryParse(accessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
