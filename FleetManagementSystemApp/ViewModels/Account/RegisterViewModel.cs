using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Не указан адрес электронной почты")]
    [EmailAddress(ErrorMessage = "Некорректный адрес")]
    [Display(Name = "E-mail")]
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указано имя")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Введите имя (от 2 до 50 символов)")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указано отчество")]
    [StringLength(50, ErrorMessage = "Введите отчество (до 50 символов)")]
    [Display(Name = "Отчество")]
    public string MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указана фамилия")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Введите фамилию (от 2 до 50 символов)")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

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

    [Required(ErrorMessage = "Не указано название организации")]
    [StringLength(50,
        ErrorMessage = "Введите название организации (до 50 символов)")]
    [Display(Name = "Название организации")]
    public string CompanyName { get; set; } = string.Empty;

    //[RegularExpression(@"^\+[1-9]\d{3}[-]?\d{3}[-]?\d{4}$")]
    //[StringLength(18, MinimumLength = 12, ErrorMessage = "Номер телефона должен состоять из 11 цифр")]
    [Display(Name = "Телефон")]
    [Phone]
    public string PhoneNum { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указано название региона")]
    [Display(Name = "Регион")]
    [StringLength(50, ErrorMessage = "Введите название региона (до 50 символов)")]
    public string Region { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указано название населенного пункта")]
    [Display(Name = "Город")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Введите название населенного пункта (до 50 символов)")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указано название улицы")]
    [Display(Name = "Улица")]
    [StringLength(50, ErrorMessage = "Введите название улицы (до 50 символов)")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Не указано название дома")]
    [Display(Name = "Дом")]
    [StringLength(4, ErrorMessage = "Введите номер дома (до 4 символов)")]
    public string House { get; set; } = string.Empty;

    [Display(Name = "Строение")]
    [StringLength(5, ErrorMessage = "Укажите строение (до 5 символов")]
    public string? Building { get; set; }

    [Display(Name = "Квартира")]
    [StringLength(5, ErrorMessage = "Укажите строение (до 5 символов")]
    public string? Apartment { get; set; }

    // TODO: для тестирования отключена валидация полей

    [Display(Name = "ИНН")]
    //[RegularExpression(@"^\d{10}$|^\d{12}$", ErrorMessage = "ИНН должен содержать 10 (ИП) или 12 (юр. лицо) символов")]
    public string Inn { get; set; } = string.Empty;

    [Display(Name = "КПП")]
    //[RegularExpression(@"^\d{9}$", ErrorMessage = "КПП должен содержать 9 символов")]
    public string? Kpp { get; set; }

    [Display(Name = "ОГРН")]
    //[RegularExpression(@"^\d{13}$", ErrorMessage = "ОГРН должен содержать 13 символов")]
    public string? Ogrn { get; set; }

    [Display(Name = "ОКПО")]
    //[RegularExpression(@"^\d{7}$|^\d{9}$", ErrorMessage = "ОКПО должен содержать 7 или 9 символов")]
    public string? Okpo { get; set; }
}