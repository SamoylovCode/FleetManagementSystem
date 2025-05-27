using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.ViewModels.Account;
using FleetManagementSystemApp.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IUserService
{
    public ValueTask<Result<List<ApplicationUserDto>>> GetAllUsersListAsync();
    public ValueTask<Result<ApplicationUserDto>> GetUserByIdAsync(string id);
    public ValueTask<Result<ApplicationUserDto>> GetUserByEmailAsync(string email);
    public ValueTask<IdentityResult> AddUserAsync(AddUserViewModel user, string callbackUrl);
    public ValueTask<IdentityResult> CreateUserAsync(RegisterViewModel model, string callbackUrl);
    public ValueTask<IdentityResult> LoginUserAsync(LoginViewModel model);
    public ValueTask<IdentityResult> SetPasswordAsync(SetPassword model);

    //public ValueTask<IdentityResult> UpdateUserDataAsync(UpdateUserDataViewModel model);
    //public ValueTask<IdentityResult> ResetUserPasswordAsync(ResetPasswordViewModel model);
}