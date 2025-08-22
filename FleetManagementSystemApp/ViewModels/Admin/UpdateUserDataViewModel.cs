using FleetManagementSystemApp.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Admin;

public class UpdateUserDataViewModel
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

    [StringLength(50, ErrorMessage = "Введите отчество (до 50 символов)")]
    [Display(Name = "Отчество")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Не указана фамилия")]
    [StringLength(
        maximumLength: 50,
        MinimumLength = 2,
        ErrorMessage = "Введите фамилию (от 2 до 50 символов)")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указана роль пользователя")]
    [Display(Name = "Роль")]
    public ApplicationRole Role { get; set; }
}