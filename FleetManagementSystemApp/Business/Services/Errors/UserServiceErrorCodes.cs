namespace FleetManagementSystemApp.Business.Services.Errors;

public static class UserServiceErrorCodes
{
    public const string CompanyNotFound = "CompanyNotFound";
    public const string CompanyHasNoEmployees = "CompanyHasNoEmployees";
    public const string UserNotFound = "UserNotFound";
    public const string UserIdIsNullOrEmpty = "UserIdIsNullOrEmpty";
    public const string UserCreationFailed = "UserCreationFailed";
    public const string PasswordDoesNotMatch = "PasswordDoesNotMatch";
    public const string AddPasswordFailed = "AddPasswordFailed";
    public const string EmailNotFound = "EmailNotFound";
    public const string EmailIsNullOrEmpty = "EmailIsNullOrEmpty";
    public const string SendEmailFailed = "SendEmailFailed";
    public const string EmailConfirmedFailed = "EmailConfirmedFailed";
    public const string EmailNotConfirmed = "EmailNotConfirmed";
    public const string AddToRoleFailed = "AddToRoleFailed";
    public const string AddClaimsFailed = "AddClaimsFailed";
}