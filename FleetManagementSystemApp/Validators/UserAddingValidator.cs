using FleetManagementSystemApp.ViewModels.Admin;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace FleetManagementSystemApp.Validators;

public class UserAddingValidator : AbstractValidator<AddUserViewModel>
{
	private readonly RoleManager<IdentityRole> _roleManager;
    public UserAddingValidator(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;

        RuleFor(m => m.FirstName).NotEmpty().WithMessage("Не указано имя сотрудника.");
        RuleFor(m => m.MiddleName).NotEmpty().WithMessage("Не указано отчество сотрудника.");
        RuleFor(m => m.LastName).NotEmpty().WithMessage("Не указана фамилия сотрудника.");
        RuleFor(m => m.Email).NotEmpty().WithMessage("Не указан адрес электронной почты.")
            .EmailAddress().WithMessage("Некорректный формат адреса электронной почты.");
        //RuleFor(m => m.Role).NotEmpty().WithMessage("Не указана роль.")
        //    .MustAsync(async (role, token) => await _roleManager.RoleExistsAsync(role))
        //        .WithMessage("Указанная роль не существует.");
    }
}