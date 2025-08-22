namespace FleetManagementSystemApp.Infrastructure.ModelBinders.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class DateRangeBinderAttribute : Attribute
{
    public readonly string _startDateProperty;
    public readonly string _endDateProperty;

    public DateRangeBinderAttribute(string startDateProperty, string endDateProperty)
    {
        _startDateProperty = startDateProperty;
        _endDateProperty = endDateProperty;
    }
}