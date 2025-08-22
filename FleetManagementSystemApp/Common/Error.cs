namespace FleetManagementSystemApp.Common;

public sealed record Error
{
    public string? Code { get; set; } = string.Empty;
    public string? UserDescription { get; set; } = string.Empty;
    public string DevDescription { get; set; }
    public object? StructuredLogContext { get; set; } = string.Empty;
    
    public static readonly Error None = new Error(string.Empty, string.Empty, string.Empty, null);

    public Error(
        string? code = null,
        string? userDesc = null,
        string devDesc = "",
        object? context = null)
    {
        Code = code;
        UserDescription = userDesc;
        DevDescription = devDesc;
        StructuredLogContext = context;
    }
}