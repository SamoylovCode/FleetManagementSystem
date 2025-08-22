namespace FleetManagementSystemApp.Validators
{
    public interface ICurrentUserService
    {
        string UserName { get; }
        string UserId { get; }
        string CompanyId { get; }
        Guid CompanyGuid { get; }
        string UserRole { get; }
        string CompanyName { get; }
    }
}