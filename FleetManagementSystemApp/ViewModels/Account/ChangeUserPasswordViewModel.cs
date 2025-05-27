using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FleetManagementSystemApp.ViewModels.Account
{
    public class ChangeUserPasswordViewModel
    {
        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        [UIHint("Password")]
        [DisplayName("Пароль")]
        public string Password { get; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [UIHint("Password")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DisplayName("Введите пароль еще раз")]
        public string PasswordConfirm { get; set; } = string.Empty;
    }
}
