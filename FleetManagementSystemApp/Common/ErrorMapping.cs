using FleetManagementSystemApp.Business.Services.Errors;

namespace FleetManagementSystemApp.Common;

/// <summary>
/// Provides mapping between error codes and corresponding view responses.
/// </summary>
public class ErrorMapping
{
    /// <summary>
    /// Gets the dictionary mapping error codes to view responses.
    /// </summary>
    /// <value>
    /// Dictionary where:
    /// - Key is the error code (string)
    /// - Value is a tuple containing:
    ///   - ViewName: name of the view to display
    ///   - StatusCode: HTTP status code to return
    /// </value>
    public Dictionary<string, (string ViewName, int StatusCode)> ViewError { get; } = new()
    {
        [nameof(UserServiceErrorCodes.CompanyNotFound)] = ("NotFound", 400),
        [nameof(UserServiceErrorCodes.UserNotFound)] = ("NotFound", 404),
        [nameof(CommonErrorCodes.ParamIsNullOrEmpty)] = ("Error", 400),
        [nameof(CommonErrorCodes.ConcurrencyConflict)] = ("Error", 409)
    };
}