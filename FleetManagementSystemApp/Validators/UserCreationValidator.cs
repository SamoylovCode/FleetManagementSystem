using FleetManagementSystemApp.Data.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace FleetManagementSystemApp.Validators;

/// <summary>
/// Validation rules for creating users
/// </summary>
public class UserCreationValidator : AbstractValidator<(ApplicationUser user, string? password, string role)>
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserCreationValidator(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;

        RuleFor(x => x.user).NotEmpty().WithMessage("Не указан пользователь.");
        When(x =>
        {
            var (_, _, role) = x;
            return role == ApplicationRole.Admin;
        }, () =>
        {
            RuleFor(x => x.password).NotEmpty().WithMessage("Не указан пароль.");
        });
        RuleFor(x => x.role).NotEmpty().WithMessage("Не указана роль.")
            .MustAsync(async (role, _) => await _roleManager.RoleExistsAsync(role))
                .WithMessage("Указанная роль не существует.");
        RuleFor(x => x.user.Email).EmailAddress().WithMessage("Некорректный формат адреса электронной почты.")
            .NotEmpty().WithMessage("Укажите адрес эектронной почты.")
            .MustAsync(async (email, _) => await _userManager.FindByEmailAsync(email) is null)
                .WithMessage("Пользователь с указанным адресом электронной почты уже существует.");
    }
}