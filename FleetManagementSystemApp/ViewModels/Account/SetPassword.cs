using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Account;

public class SetPassword
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; }

    [Required(ErrorMessage = "Не указан пароль")]
    [DataType(DataType.Password)]
    [UIHint("Password")]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [UIHint("Password")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Введите пароль еще раз")]
    public string PasswordConfirm { get; set; } = string.Empty;
}