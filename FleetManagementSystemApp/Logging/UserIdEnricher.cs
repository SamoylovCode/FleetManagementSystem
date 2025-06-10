using Serilog.Core;
using Serilog.Events;
using System.Security.Claims;

namespace FleetManagementSystemApp.Logging;

public class UserIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly string _propertyName;

    public UserIdEnricher(IHttpContextAccessor httpContextAccessor, string propertyName = "UserId")
    {
        _contextAccessor = httpContextAccessor;
        _propertyName = propertyName;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext is null)
        {
            return;
        }

        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is not null)
            {
                if (Guid.TryParse(userIdClaim.Value, out var userIdGuid))
                {
                    var prop = propertyFactory.CreateProperty(_propertyName, userIdGuid);
                    logEvent.AddOrUpdateProperty(prop);
                }
                else
                {
                    var prop = propertyFactory.CreateProperty(_propertyName, userIdClaim.Value);
                    logEvent.AddOrUpdateProperty(prop);
                }
            }
        }
    }
}
