using FleetManagementSystemApp.ViewModels.Account;
using FluentValidation;

namespace FleetManagementSystemApp.Validators
{
    public class UserLoginValidator : AbstractValidator<LoginViewModel>
    {
        public UserLoginValidator()
        {
            RuleFor(m => m.Password).NotEmpty().WithMessage("Не указан пароль.");
            RuleFor(m => m.Email).NotEmpty().WithMessage("Не указан адрес электронной почты.");
            RuleFor(m => m.Email).EmailAddress().WithMessage("Некорректный формат адреса электронной почты.");
        }
    }
}
