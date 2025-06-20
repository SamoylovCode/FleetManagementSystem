using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FleetManagementSystemApp.Common;

public class ServiceResultActionResult<T> : IActionResult
{
    private readonly Result<T> _result;
    private readonly ITempDataDictionaryFactory _tempData;
    private readonly ErrorMapping _errorMapping;

    public ServiceResultActionResult(Result<T> result, ITempDataDictionaryFactory tempData, ErrorMapping errorMapping)
    {
        _result = result;
        _tempData = tempData;
        _errorMapping = errorMapping;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var metadataProvider = context.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();

        if (_result.IsSuccess)
        {
            var viewResult = new ViewResult
            {
                ViewName = context.ActionDescriptor.RouteValues["action"],
                ViewData = new ViewDataDictionary<T>(metadataProvider, context.ModelState)
                {
                    Model = _result.Value
                },
                TempData = _tempData.GetTempData(context.HttpContext)
            };

            await viewResult.ExecuteResultAsync(context);
            return;
        }

        if(_errorMapping.ViewError.TryGetValue(_result.Error.Code, out var dictionary))
        {
            context.HttpContext.Response.StatusCode = dictionary.StatusCode;

            var viewResult = new ViewResult
            {
                ViewName = dictionary.ViewName,
                ViewData = new ViewDataDictionary<string>(metadataProvider,context.ModelState)
                {
                    Model = _result.Error.DevDescription
                },
                TempData = _tempData.GetTempData(context.HttpContext)
            };

            await viewResult.ExecuteResultAsync(context);
        }
        else
        {
            context.HttpContext.Response.StatusCode = 500;
            await new ViewResult
            {
                ViewName = "Error"
            }.ExecuteResultAsync(context);
        }
    }
}
