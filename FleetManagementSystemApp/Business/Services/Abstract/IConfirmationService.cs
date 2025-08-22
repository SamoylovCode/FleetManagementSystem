using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IConfirmationService
{
    public Task<Result> SendConfirmationAsync(ApplicationUser user, string scheme);
}
