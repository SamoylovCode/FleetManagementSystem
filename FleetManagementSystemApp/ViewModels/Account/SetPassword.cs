using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Account;

public class SetPassword
{
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указан пароль")]
    [DataType(DataType.Password)]
    [UIHint("Password")]
    [DisplayName("Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [UIHint("Password")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [DisplayName("Введите пароль еще раз")]
    public string PasswordConfirm { get; set; } = string.Empty;
}