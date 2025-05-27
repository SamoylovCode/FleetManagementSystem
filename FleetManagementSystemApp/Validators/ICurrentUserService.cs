using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Validators
{
    public interface ICurrentUserService
    {
        string UserName { get; }
        string UserId { get; }
        string CompanyId { get; }
        string UserRole { get; }
    }
}
