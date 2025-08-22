using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Account;

public class ChangeUserPasswordViewModel
{
    [Required(ErrorMessage = "Не указан пароль")]
    [DataType(DataType.Password)]
    [UIHint("Password")]
    [Display(Name = "Пароль")]
    public string Password { get; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [UIHint("Password")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Введите пароль еще раз")]
    public string PasswordConfirm { get; set; } = string.Empty;
}