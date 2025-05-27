namespace FleetManagementSystemApp.Data.Entities;

public class ApplicationRole
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string Dispatcher = "dispatcher";
    public const string Inspector = "inspector";

    public static IReadOnlyDictionary<string, string> AllRoles = new Dictionary<string, string>
    {
        [Manager] = "руководитель",
        [Dispatcher] = "диспетчер",
        [Inspector] = "контролер"
    };
}