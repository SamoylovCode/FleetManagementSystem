using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.ViewModels.Account;
using FleetManagementSystemApp.ViewModels.Admin;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IUserService
{
    public Task<Result<List<ApplicationUserDto>>> GetAllUsersListAsync();
    public Task<Result<ApplicationUserDto>> GetUserByIdAsync(string id);
    public Task<Result<ApplicationUserDto>> GetUserByEmailAsync(string email);
    public Task<Result> AddUserAsync(AddUserViewModel user, string callbackUrl);
    public Task<Result> CreateUserAsync(RegisterViewModel model, string callbackUrl);
    public Task<Result> LoginUserAsync(LoginViewModel model);
    public Task<Result> SetPasswordAsync(SetPassword model);

    //public ValueTask<Result> UpdateUserDataAsync(UpdateUserDataViewModel model);
    //public ValueTask<Result> ResetUserPasswordAsync(ResetPasswordViewModel model);
}