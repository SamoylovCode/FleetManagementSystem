namespace FleetManagementSystemApp.Common;

public enum UserServiceErrorEnum
{
    CompanyNotFound,
    CompanyHasNoEmployees,
    UserNotFound,
    UserIdIsNullOrEmpty,
    UserCreationFailed,
    PasswordDoesNotMatch,
    AddPasswordFailed,
    EmailNotFound,
    EmailIsNullOrEmpty,
    SendEmailFailed,
    EmailConfirmedFailed,
    AddToRoleFailed,
    AddClaimsFailed
}