namespace FleetManagementSystemApp.Common;

public class ErrorMapping
{
    public Dictionary<string, (string ViewName, int StatusCode)> ViewError { get; } = new()
    {
        [nameof(UserServiceErrorEnum.CompanyNotFound)] = ("NotFound", 400),
        [nameof(UserServiceErrorEnum.UserNotFound)] = ("NotFound", 404)
    };
}
