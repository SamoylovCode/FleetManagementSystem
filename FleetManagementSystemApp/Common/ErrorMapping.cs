using FleetManagementSystemApp.Business.Services.Errors;

namespace FleetManagementSystemApp.Common;

public class ErrorMapping
{
    public Dictionary<string, (string ViewName, int StatusCode)> ViewError { get; } = new()
    {
        [nameof(UserServiceErrorCodes.CompanyNotFound)] = ("NotFound", 400),
        [nameof(UserServiceErrorCodes.UserNotFound)] = ("NotFound", 404)
    };
}