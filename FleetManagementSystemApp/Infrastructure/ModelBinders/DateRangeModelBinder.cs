using FleetManagementSystemApp.Infrastructure.ModelBinders.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;

namespace FleetManagementSystemApp.Infrastructure.ModelBinders;

public class DateRangeModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        // Fallback to default model binding
        if (bindingContext is null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var factory = bindingContext.HttpContext.RequestServices.GetRequiredService<IModelBinderFactory>();
        var metaDataProvider = bindingContext.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();

        var metaData = bindingContext.ModelMetadata;

        var fallbackBinder = factory.CreateBinder(new ModelBinderFactoryContext
        {
            BindingInfo = new BindingInfo(),
            Metadata = metaData,
            CacheToken = metaData
        });

        await fallbackBinder.BindModelAsync(bindingContext);

        // Custom attribute
        if (bindingContext.Result.IsModelSet)
        {
            var model = bindingContext.Result.Model;

            if (model is null)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Model is null; cannot apply DateRangeBinderAttribute.");
                return;
            }

            foreach (var prop in model.GetType().GetProperties())
            {
                var attr = prop.GetCustomAttribute<DateRangeBinderAttribute>();

                if (attr is null)
                {
                    continue;
                }

                var periodStringObj = prop.GetValue(model);
                if (periodStringObj is not string periodString || string.IsNullOrWhiteSpace(periodString))
                {
                    continue;
                }

                var dates = periodString.Split(" - ");

                if (dates.Length != 2 ||
                    !DateOnly.TryParseExact(dates[0], "dd.MM.yyyy", out DateOnly startDate) ||
                    !DateOnly.TryParseExact(dates[1], "dd.MM.yyyy", out DateOnly endDate))
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Failed to parse period string. Expected format: dd.MM.yyyy - dd.MM.yyyy.");
                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }

                var startProp = model.GetType().GetProperty(attr._startDateProperty);
                var endProp = model.GetType().GetProperty(attr._endDateProperty);

                startProp?.SetValue(model, startDate);
                endProp?.SetValue(model, endDate);
            }
        }

        bindingContext.Result = ModelBindingResult.Success(bindingContext.Result.Model);
    }
}