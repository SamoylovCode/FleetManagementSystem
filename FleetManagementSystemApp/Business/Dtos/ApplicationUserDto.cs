namespace FleetManagementSystemApp.Business.Dtos;

/// <summary>
/// Represents user data transfer object (DTO)
/// </summary>
public class ApplicationUserDto
{
    public string UserId { get; init; }
    public string FirstName { get; init; }
    public string MiddleName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string NormalizedEmail { get; init; }
    public bool EmailConfirmed { get; init; }
    public string PasswordHash { get; init; }
    public string SecurityStamp { get; init; }

    public ApplicationUserDto() { }

    public ApplicationUserDto(
        string userId,
        string firstName,
        string middleName,
        string lastName,
        string email,
        string normalizedEmail,
        bool emailConfirmed,
        string passwordHash,
        string securityStamp)
    {
        UserId = userId;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Email = email;
        NormalizedEmail = normalizedEmail;
        EmailConfirmed = emailConfirmed;
        PasswordHash = passwordHash;
        SecurityStamp = securityStamp;
    }
}