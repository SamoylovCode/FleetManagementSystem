using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Не указан адрес электронной почты")]
    [Display(Name = "E-mail")]
    [EmailAddress(ErrorMessage = "Некоректный адрес")]
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указан пароль")]
    [DataType(DataType.Password)]
    [UIHint("Password")]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}