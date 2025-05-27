using FleetManagementSystemApp.Data.Entities;
using System.Security.Claims;

namespace FleetManagementSystemApp.Validators
{
    public class CurrentUserService : ICurrentUserService
    {
        readonly IHttpContextAccessor _contextAccessor;

        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string UserName => _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
        public string UserId => _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        public string CompanyId => _contextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value ?? string.Empty;
        public string UserRole => _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}