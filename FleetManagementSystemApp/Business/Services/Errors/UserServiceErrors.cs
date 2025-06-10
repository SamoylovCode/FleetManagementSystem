using FleetManagementSystemApp.Common;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace FleetManagementSystemApp.Business.Services.Errors;

public class UserServiceErrors
{
    public static Error CompanyNotFound(string companyId) =>
        new Error(UserServiceErrorEnum.CompanyNotFound,
            string.IsNullOrEmpty(companyId)
                ? "Компания не определена в контексте пользователя."
                : $"Компания '{companyId}' не найдена.");
    public static Error CompanyHasNoEmployees(string companyId) =>
        new Error(UserServiceErrorEnum.CompanyHasNoEmployees, $"В указанной организации '{companyId}' не содержится ни одного сотрудника.");
    public static Error UserNotFound(string userId) =>
        new Error(UserServiceErrorEnum.UserNotFound, $"Пользователь '{userId}' не найден");
    public static Error UserIdIsNullOrEmpty() =>
        new Error(UserServiceErrorEnum.UserIdIsNullOrEmpty, "ID пользователя не указан.");
    public static Error UserCreationFailed() =>
        new Error(UserServiceErrorEnum.UserCreationFailed, "Ошибка при создании пользователя.");
    public static Error PasswordDoesNotMatch(string userId) =>
        new Error(UserServiceErrorEnum.PasswordDoesNotMatch, $"Пароль учетной записи пользователя '{userId}' не соответствует.");
    public static Error AddPasswordFailed(string userId) =>
        new Error(UserServiceErrorEnum.AddPasswordFailed, $"Не удалось сохранить пароль учетной записи пользователя '{userId}'");
    public static Error EmailNotFound() =>
        new Error(UserServiceErrorEnum.EmailNotFound, $"Email не найден.");
    public static Error EmailIsNullOrEmpty() =>
        new Error(UserServiceErrorEnum.EmailIsNullOrEmpty, $"Email не указан.");
    public static Error SendEmailFailed(string userId) =>
        new Error(UserServiceErrorEnum.SendEmailFailed, $"Не удалось отправить письмо с подтверждением регистрации пользователя '{userId}'.");
    public static Error EmailConfirmedFailed(string userId) =>
        new Error(UserServiceErrorEnum.EmailConfirmedFailed, $"Не удалось подтвердить почту пользователя '{userId}'.");
    public static Error AddToRoleFailed(string userId) =>
        new Error(UserServiceErrorEnum.AddToRoleFailed, $"Не удалось применить роль к учетной записи пользователя '{userId}'.");
    public static Error AddClaimsFailed(string userId) =>
        new Error(UserServiceErrorEnum.AddClaimsFailed, $"Не удалось применить утверждения к учетной записи пользователя '{userId}'");
}
