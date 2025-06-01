using FleetManagementSystemApp.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IConfirmationService
{
    public Task<IdentityResult> SendConfirmationAsync(ApplicationUser user, string scheme);
}
