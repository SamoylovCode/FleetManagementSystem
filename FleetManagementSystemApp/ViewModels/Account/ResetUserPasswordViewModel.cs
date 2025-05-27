using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace FleetManagementSystemApp.ViewModels.Account;

public class ResetUserPasswordViewModel
{
    [Required(ErrorMessage = "Не указан адрес электронной почты")]
    [EmailAddress(ErrorMessage = "Некорректный адрес")]
    [DisplayName("E-mail")]
    [StringLength(50)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Не указан код, отправленный на почту пользователя")]
    [StringLength(6)]
    public string VerificationCode { get; set; } = string.Empty;
}