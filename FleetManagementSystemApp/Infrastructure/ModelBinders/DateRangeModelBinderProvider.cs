using FleetManagementSystemApp.Infrastructure.ModelBinders.Attributes;
using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Reflection;

namespace FleetManagementSystemApp.Infrastructure.ModelBinders;

public class DateRangeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var modelType = bindingContext.Metadata.ModelType;
        
        if (!bindingContext.Metadata.IsComplexType && !typeof(ISubModel).IsAssignableFrom(modelType))
        {
            return null;
        }

        // Если модель не содержит свойств с DateRangeBinderAttribute — переход к следующей
        var hasDateRange = bindingContext.Metadata.IsComplexType &&
                           bindingContext.Metadata.ContainerType != null &&
                           bindingContext.Metadata.ContainerType.GetProperty(bindingContext.Metadata.PropertyName)
                               ?.GetCustomAttribute<DateRangeBinderAttribute>() != null;


        if (!hasDateRange)
        {
            return null;
        }

        return new BinderTypeModelBinder(typeof(DateRangeModelBinder));
    }
}