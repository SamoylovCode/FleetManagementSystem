using FleetManagementSystemApp.Common;

namespace FleetManagementSystemApp.Business.Services.Errors;

public static class UserServiceErrors
{
    public static Error CompanyNotFound(string companyId) =>
        new Error(
            UserServiceErrorCodes.CompanyNotFound,
            userDesc: "Не найдена компания.",
            devDesc: string.IsNullOrEmpty(companyId)
                ? "Сompany ID is null or empty."
                : $"Сompany '{companyId}' not found.",
            context: new { CompanyId = companyId });

    public static Error CompanyHasNoEmployees(string companyId) =>
        new Error(
            UserServiceErrorCodes.CompanyHasNoEmployees,
            userDesc: "В компании пока нет сотрудников.",
            devDesc: $"No employees found for company '{companyId}'.",
            context: new { CompanyId = companyId });

    public static Error UserNotFoundById(string userId) =>
        new Error(
            UserServiceErrorCodes.UserNotFound,
            userDesc: "Пользователь с таким идентификатором не найден.",
            devDesc: $"User with ID '{userId}' not found.",
            context: new { UserId = userId });

    public static Error UserNotFoundByEmail(string email) =>
        new Error(
            UserServiceErrorCodes.UserNotFound,
            userDesc: "Пользователь с таким email не найден.",
            devDesc: $"User with email '{email}' not found.",
            context: new { UserEmail = email });

    public static Error UserIdIsNullOrEmpty() =>
        new Error(
            UserServiceErrorCodes.UserIdIsNullOrEmpty,
            userDesc: "Идентификатор пользователя не указан.",
            devDesc: "UserId is null or empty.",
            context: null);

    public static Error UserCreationFailed() =>
        new Error(
            UserServiceErrorCodes.UserCreationFailed,
            userDesc: "Не удалось зарегистрировать пользователя.",
            devDesc: "Failed to create new user in database.",
            context: null );

    public static Error PasswordDoesNotMatch(string userId) =>
        new Error(
            UserServiceErrorCodes.PasswordDoesNotMatch,
            userDesc: "Неверный пароль.",
            devDesc: $"Invalid password for user '{userId}'.",
            context: new { UserId = userId });

    public static Error AddPasswordFailed(string userId) =>
        new Error(
            UserServiceErrorCodes.AddPasswordFailed,
            userDesc: "Не удалось сохранить пароль.",
            devDesc: $"Could not set password for user '{userId}'.",
            context: new { UserId = userId });

    public static Error EmailNotFound(string maskedEmail) =>
        new Error(
            UserServiceErrorCodes.EmailNotFound,
            userDesc: $"Пользователь с таким email {maskedEmail} не найден.",
            devDesc: "Email address not found in user store.",
            context: new { UserEmail = maskedEmail });

    public static Error EmailIsNullOrEmpty() =>
        new Error(
            UserServiceErrorCodes.EmailIsNullOrEmpty,
            userDesc: "Email не указан.",
            devDesc: "Email is null or empty.",
            context: null);

    public static Error SendEmailFailed(string userId) =>
        new Error(
            UserServiceErrorCodes.SendEmailFailed,
            userDesc: "Не удалось отправить письмо.",
            devDesc: $"Failed to send confirmation email to user '{userId}'.",
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
            devDesc: $"Cannot confirm email for user '{userId}'.",
            context: new { UserId = userId });
    
    public static Error EmailNotConfirmed(string userId) =>
    new Error(
        UserServiceErrorCodes.EmailNotConfirmed,
        userDesc: "Почта пользователя не подтверждена.",
        devDesc: $"Email for user '{userId}' is not confirmed.",
        context: new { UserId = userId });

    public static Error AddToRoleFailed(string userId) =>
        new Error(
            UserServiceErrorCodes.AddToRoleFailed,
            userDesc: "Не удалось назначить роль.",
            devDesc: $"Failed to add role to user '{userId}'.",
            context: new { UserId = userId });

    public static Error AddClaimsFailed(string userId) =>
        new Error(
            UserServiceErrorCodes.AddClaimsFailed,
            userDesc: "Не удалось сохранить данные доступа.",
            devDesc: $"Failed to apply claims to user '{userId}'.",
            context: new { UserId = userId });
}
