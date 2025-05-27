using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace FleetManagementSystemApp.Business.Services;

public class ConfirmationEmailService : IConfirmationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ConfirmationEmailService(UserManager<ApplicationUser> userManager,
                                    ApplicationDbContext dbContext,
                                    SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> ConfirmAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("$\"Пользователь с ID '{userId}' не найден.\"");
        }

        var confirmationResult = await _userManager.ConfirmEmailAsync(user, token);
        if (!confirmationResult.Succeeded)
        {
            throw new Exception("Не удалось отправить письмо с подтверждением регистрации.");
        }

        if (await _userManager.HasPasswordAsync(user))
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
        }

        return false;
    }
}
