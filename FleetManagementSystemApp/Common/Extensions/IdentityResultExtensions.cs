using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagementSystemApp.Common.Extensions;

public static class IdentityResultExtensions
{
    public static T Match<T>(
        this IdentityResult result,
        Func<T> onSuccess,
        Func<IEnumerable<IdentityError>, T> onFailure)
    {
        return result.Succeeded
            ? onSuccess()
            : onFailure(result.Errors);
    }

    public static IActionResult ToActionIdentityResult(
        this IdentityResult result,
        Func<IActionResult> onSuccess,
        Func<IEnumerable<IdentityError>, IActionResult> onFailure)
    {
        return result.Succeeded
            ? onSuccess()
            : onFailure(result.Errors);
    }

    public static async Task<IActionResult> ToActionIdentityResultAsync(
    this IdentityResult result,
    Func<Task<IActionResult>> onSuccess,
    Func<IEnumerable<IdentityError>, Task<IActionResult>> onFailure)
    {
        return result.Succeeded
            ? await onSuccess()
            : await onFailure(result.Errors);
    }
}