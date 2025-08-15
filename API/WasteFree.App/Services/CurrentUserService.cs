using System.Security.Claims;
using WasteFree.Shared.Interfaces;

namespace WasteFree.App.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId => GetCurrentUserId();
    
    public Guid GetCurrentUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?
            .User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}