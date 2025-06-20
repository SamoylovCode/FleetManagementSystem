using FleetManagementSystemApp.Common;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace FleetManagementSystemApp.Business.Services.Errors;
public static class UserServiceErrors
{
    public static Error CompanyNotFound(string companyId) =>
        new Error(
            UserServiceErrorCodes.CompanyNotFound,
            userDesc: "Не найдена компания.",
            devDesc: string.IsNullOrEmpty(companyId)
                ? "CompanyNotFound: companyId is null or empty."
                : $"CompanyNotFound: company '{companyId}' not found.",
            context: new { CompanyId = companyId });

    public static Error CompanyHasNoEmployees(string companyId) =>
        new Error(
            UserServiceErrorCodes.CompanyHasNoEmployees,
            userDesc: "В компании пока нет сотрудников.",
            devDesc: $"CompanyHasNoEmployees: no employees found for company '{companyId}'.",
            context: new { CompanyId = companyId });

    public static Error UserNotFoundById(string userId) =>
        new Error(
            UserServiceErrorCodes.UserNotFound,
            userDesc: "Пользователь с таким идентификатором не найден.",
            devDesc: $"UserNotFound: user with ID '{userId}' not found.",
            context: new { UserId = userId });
    
    public static Error UserNotFoundByEmail(string email) =>
        new Error(
            UserServiceErrorCodes.UserNotFound,
            userDesc: "Пользователь с таким email не найден.",
            devDesc: $"UserNotFound: user with email '{email}' not found.",
            context: new { UserEmail = email });

    public static Error UserIdIsNullOrEmpty() =>
        new Error(
            UserServiceErrorCodes.UserIdIsNullOrEmpty,
            userDesc: "Идентификатор пользователя не указан.",
            devDesc: "UserIdIsNullOrEmpty: userId is null or empty.",
            context: null);

    public static Error UserCreationFailed() =>
        new Error(
            UserServiceErrorCodes.UserCreationFailed,
            userDesc: "Не удалось зарегистрировать пользователя.",
            devDesc: "UserCreationFailed: failed to create new user in database.",
            context: null );

    public static Error PasswordDoesNotMatch(string userId) =>
        new Error(
            UserServiceErrorCodes.PasswordDoesNotMatch,
            userDesc: "Неверный пароль.",
            devDesc: $"PasswordDoesNotMatch: invalid password for user '{userId}'.",
            context: new { UserId = userId });

    public static Error AddPasswordFailed(string userId) =>
        new Error(
            UserServiceErrorCodes.AddPasswordFailed,
            userDesc: "Не удалось сохранить пароль.",
            devDesc: $"AddPasswordFailed: could not set password for user '{userId}'.",
            context: new { UserId = userId });

    public static Error EmailNotFound(string maskedEmail) =>
        new Error(
            UserServiceErrorCodes.EmailNotFound,
            userDesc: $"Пользователь с таким email {maskedEmail} не найден.",
            devDesc: "EmailNotFound: email address not found in user store.",
            context: new { UserEmail = maskedEmail });

    public static Error EmailIsNullOrEmpty() =>
        new Error(
            UserServiceErrorCodes.EmailIsNullOrEmpty,
            userDesc: "Email не указан.",
            devDesc: "EmailIsNullOrEmpty: email is null or empty.",
            context: null);

    public static Error SendEmailFailed(string userId) =>
        new Error(
            UserServiceErrorCodes.SendEmailFailed,
            userDesc: "Не удалось отправить письмо.",
            devDesc: $"SendEmailFailed: failed to send confirmation email to user '{userId}'.",
            context: new { UserId = userId });

    public static Error SendEmailFailed(Exception ex) =>
        new Error(
            UserServiceErrorCodes.SendEmailFailed,
            userDesc: "Не удалось подтвердить почту пользователя.",
            devDesc: $"{ex.Message}\nStackTrace: {ex.StackTrace}",
            context: null);

    public static Error EmailConfirmedFailed(string userId) =>
        new Error(
            UserServiceErrorCodes.EmailConfirmedFailed,
            userDesc: "Не удалось подтвердить почту пользователя.",
            devDesc: $"EmailConfirmedFailed: cannot confirm email for user '{userId}'.",
            context: new { UserId = userId });

    public static Error AddToRoleFailed(string userId) =>
        new Error(
            UserServiceErrorCodes.AddToRoleFailed,
            userDesc: "Не удалось назначить роль.",
            devDesc: $"AddToRoleFailed: failed to add role to user '{userId}'.",
            context: new { UserId = userId });

    public static Error AddClaimsFailed(string userId) =>
        new Error(
            UserServiceErrorCodes.AddClaimsFailed,
            userDesc: "Не удалось сохранить данные доступа.",
            devDesc: $"AddClaimsFailed: failed to apply claims to user '{userId}'.",
            context: new { UserId = userId });
}
