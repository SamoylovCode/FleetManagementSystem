using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IConfirmationService
{
    public Task<Result> SendConfirmationAsync(ApplicationUser user, string scheme);
}
