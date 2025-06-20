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
    public ValueTask<Result> AddUserAsync(AddUserViewModel user, string callbackUrl);
    public ValueTask<Result> CreateUserAsync(RegisterViewModel model, string callbackUrl);
    public ValueTask<Result> LoginUserAsync(LoginViewModel model);
    public ValueTask<Result> SetPasswordAsync(SetPassword model);

    //public ValueTask<Result> UpdateUserDataAsync(UpdateUserDataViewModel model);
    //public ValueTask<Result> ResetUserPasswordAsync(ResetPasswordViewModel model);
}