using FleetManagementSystemApp.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FleetManagementSystemApp.Filters;

/// <summary>
/// Автоматическая обертка каждого экшена
/// </summary>
public class ServiceResultFilter
{
    private readonly ErrorMapping _mapping;
    private readonly ITempDataDictionaryFactory _tempFactory;

    public ServiceResultFilter(
        ErrorMapping mapping,
        ITempDataDictionaryFactory tempFactory)
    {
        _mapping = mapping;
        _tempFactory = tempFactory;
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is IActionResult) return;

        // Если контроллер вернул Result<T>
        var resultType = context.Result?.GetType();
        if (resultType?.IsGenericType == true &&
            resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var result = context.Result;
            // Строим generic ServiceResultActionResult<T>
            var actionResultType = typeof(ServiceResultActionResult<>).MakeGenericType(resultType.GetGenericArguments());
            context.Result = Activator.CreateInstance(actionResultType, result, _mapping, _tempFactory) as IActionResult;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
