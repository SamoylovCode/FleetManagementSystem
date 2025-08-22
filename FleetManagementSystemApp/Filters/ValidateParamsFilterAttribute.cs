using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FleetManagementSystemApp.Filters;

public class ValidateParamsFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var method = context.ActionDescriptor as ControllerActionDescriptor;
        var parameters = method?.MethodInfo.GetParameters();
        var nullParam = parameters?
            .Where(p =>
                context.ActionArguments.TryGetValue(p.Name!, out var value) &&
                value == null &&
                !p.IsOptional &&
                !p.HasDefaultValue &&
                (
                    !p.ParameterType.IsValueType ||
                    Nullable.GetUnderlyingType(p.ParameterType) != null
                )
            )
            .FirstOrDefault();

        if (nullParam is not null)
        {
            var error = CommonErrors.ParamIsNullOrEmpty();
            context.Result = new ObjectResult(Result.Failure(error))
            {
                StatusCode = 400
            };
            return;
        }

        base.OnActionExecuting(context);
    }
}