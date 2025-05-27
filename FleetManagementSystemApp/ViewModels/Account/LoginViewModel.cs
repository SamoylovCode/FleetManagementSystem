using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace FleetManagementSystemApp.ViewModels.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Не указан адрес электронной почты")]
    [DisplayName("E-mail")]
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указан пароль")]
    [DataType(DataType.Password)]
    [UIHint("Password")]
    [DisplayName("Пароль")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}