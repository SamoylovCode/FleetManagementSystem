using FleetManagementSystemApp.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FleetManagementSystemApp.ViewModels.Admin;

public class UpdateUserDataViewModel
{
    [Required(ErrorMessage = "Не указан адрес электронной почты")]
    [EmailAddress(ErrorMessage = "Некорректный адрес")]
    [DisplayName("E-mail")]
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указано имя")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Введите имя (от 2 до 50 символов)")]
    [DisplayName("Имя")]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Введите отчество (до 50 символов)")]
    [DisplayName("Отчество")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Не указана фамилия")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Введите фамилию (от 2 до 50 символов)")]
    [DisplayName("Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указана роль пользователя")]
    [DisplayName("Роль")]
    public ApplicationRole Role { get; set; }
}