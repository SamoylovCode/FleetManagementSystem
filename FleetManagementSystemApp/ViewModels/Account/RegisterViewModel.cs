using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace FleetManagementSystemApp.ViewModels.Account;

public class RegisterViewModel
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
    public string MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указана фамилия")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Введите фамилию (от 2 до 50 символов)")]
    [DisplayName("Фамилия")]
    public string LastName { get; set; } = string.Empty;

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

    [Required(ErrorMessage = "Не указано название организации")]
    [StringLength(50,
        ErrorMessage = "Введите название организации (до 50 символов)")]
    [DisplayName("Название организации")]
    public string CompanyName { get; set; } = string.Empty;

    [RegularExpression(@"^\+[1-9]\d{3}[-]?\d{3}[-]?\d{4}$")]
    [StringLength(12, MinimumLength = 12, ErrorMessage = "Номер телефона должен состоять из 11 цифр")]
    [DisplayName("Телефон")]
    [Phone]
    public string PhoneNum { get; set; } = string.Empty;

    [DisplayName("Регион")]
    [StringLength(50, ErrorMessage = "Введите название региона (до 50 символов)")]
    public string Region { get; set; } = string.Empty;

    [DisplayName("Город")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Введите название населенного пункта (до 50 символов)")]
    public string City { get; set; } = string.Empty;

    [DisplayName("Улица")]
    [StringLength(50, ErrorMessage = "Введите название улицы (до 50 символов)")]
    public string Street { get; set; } = string.Empty;

    [DisplayName("Дом")]
    [StringLength(4, ErrorMessage = "Введите номер дома (до 4 символов)")]
    public string House { get; set; } = string.Empty;

    [DisplayName("Строение")]
    [StringLength(5, ErrorMessage = "Укажите строение (до 5 символов")]
    public string? Building { get; set; }

    [DisplayName("Квартира")]
    [StringLength(5, ErrorMessage = "Укажите строение (до 5 символов")]
    public string? Apartment { get; set; }

    [DisplayName("ИНН")]
    [RegularExpression(@"^\d{10}$|^\d{12}$", ErrorMessage = "ИНН должен содержать 10 (ИП) или 12 (юр. лицо) символов")]
    public string Inn { get; set; } = string.Empty;

    [DisplayName("КПП")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "КПП должен содержать 9 символов")]
    public string? Kpp { get; set; }

    [DisplayName("ОГРН")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "ОГРН должен содержать 13 символов")]
    public string? Ogrn { get; set; }

    [DisplayName("ОКПО")]
    [RegularExpression(@"^\d{7}$|^\d{9}$", ErrorMessage = "ОКПО должен содержать 7 или 9 символов")]
    public string? Okpo { get; set; }
}