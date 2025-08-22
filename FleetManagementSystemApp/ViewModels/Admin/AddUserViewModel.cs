using System.ComponentModel.DataAnnotations;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.ViewModels.Admin;

public class AddUserViewModel
{
    [Required(ErrorMessage = "Не указан адрес электронной почты")]
    [EmailAddress(ErrorMessage = "Некорректный адрес")]
    [Display(Name = "E-mail")]
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указано имя")]
    [StringLength(
        maximumLength: 50,
        MinimumLength = 2,
        ErrorMessage = "Введите имя (от 2 до 50 символов)")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указано отчество")]
    [StringLength(50, ErrorMessage = "Введите отчество (до 50 символов)")]
    [Display(Name = "Отчество")]
    public string MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указана фамилия")]
    [StringLength(
        maximumLength: 50,
        MinimumLength = 2,
        ErrorMessage = "Введите фамилию (от 2 до 50 символов)")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указана роль пользователя")]
    [Display(Name = "Роль")]
    [AllowedValues(
        ApplicationRole.Manager,
        ApplicationRole.Dispatcher,
        ApplicationRole.Inspector,
        ErrorMessage = "Недопустимая роль")]
    public string Role { get; set; }
}