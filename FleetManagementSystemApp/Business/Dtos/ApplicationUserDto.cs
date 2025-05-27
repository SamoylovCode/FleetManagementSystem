namespace FleetManagementSystemApp.Business.Dtos;

/// <summary>
/// Represents user data transfer object (DTO)
/// </summary>
/// <param name="userId">Unique user identifier</param>
/// <param name="firstName">User's first name</param>
/// <param name="middleName">User's middle name</param>
/// <param name="lastName">User's last name</param>
/// <param name="email">User's email address</param>
/// <param name="normalizedEmail">Normalized email for case-insensitive comparison</param>
/// <param name="emailConfirmed">Indicates if email was confirmed</param>
/// <param name="passwordHash">Hashed password</param>
/// <param name="securityStamp">Security stamp for tracking changes</param>
public class ApplicationUserDto(string userId,
    string firstName,
    string middleName,
    string lastName,
    string email,
    string normalizedEmail,
    bool emailConfirmed,
    string passwordHash,
    string securityStamp)
{
    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    /// <value>
    /// The user identifier.
    /// </value>
    public string UserId { get; init; } = userId;

    /// <summary>
    /// Gets the first name.
    /// </summary>
    /// <value>
    /// The first name.
    /// </value>
    public string FirstName { get; init; } = firstName;

    /// <summary>
    /// Gets the name of the middle.
    /// </summary>
    /// <value>
    /// The name of the middle.
    /// </value>
    public string MiddleName { get; init; } = middleName;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    /// <value>
    /// The last name.
    /// </value>
    public string LastName { get; init; } = lastName;

    /// <summary>
    /// Gets the email.
    /// </summary>
    /// <value>
    /// The email.
    /// </value>
    public string Email { get; init; } = email;

    /// <summary>
    /// Gets the normalized email.
    /// </summary>
    /// <value>
    /// The normalized email.
    /// </value>
    public string NormalizedEmail { get; init; } = normalizedEmail;

    /// <summary>
    /// Gets a value indicating whether [email confirmed].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [email confirmed]; otherwise, <c>false</c>.
    /// </value>
    public bool EmailConfirmed { get; init; } = emailConfirmed;

    /// <summary>
    /// Gets the password hash.
    /// </summary>
    /// <value>
    /// The password hash.
    /// </value>
    public string PasswordHash { get; init; } = passwordHash;

    /// <summary>
    /// Gets the security stamp.
    /// </summary>
    /// <value>
    /// The security stamp.
    /// </value>
    public string SecurityStamp { get; init; } = securityStamp;
}