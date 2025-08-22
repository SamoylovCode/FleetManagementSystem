using Microsoft.AspNetCore.Mvc;

namespace FleetManagementSystemApp.Common.Extensions;

public static class ResultExtensions
{
    public static T Match<T>(
        this Result<T> result,
        Func<T> onSuccess,
        Func<IList<Error>, T> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Errors);
    }

    public static IActionResult ToActionResult(
        this Result result,
        Func<IActionResult> onSuccess,
        Func<IList<Error>, IActionResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Errors);
    }

    public static async Task<IActionResult> ToActionResultAsync(
        this Result result,
        Func<Task<IActionResult>> onSuccess,
        Func<IList<Error>, Task<IActionResult>> onFailure)
    {
        return result.IsSuccess
            ? await onSuccess()
            : await onFailure(result.Errors);
    }
}