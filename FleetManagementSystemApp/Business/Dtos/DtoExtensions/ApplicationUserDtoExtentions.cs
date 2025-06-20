using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

/// <summary>
/// Convertation methods for ApplicationUser data to and from DTO model
/// </summary>
public class ApplicationUserDtoExtentions : BaseMapper<ApplicationUser, ApplicationUserDto>
{
    /// <summary>
    /// Converts user instance to dto.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public override Result<ApplicationUserDto> ToDto(ApplicationUser user)
    {
        if (user is null)
        {
            return Result<ApplicationUserDto>.Failure(MapperErrors.ModelIsNull());
        }

        var userDto = new ApplicationUserDto
        (
            user.Id,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.Email,
            user.NormalizedEmail,
            user.EmailConfirmed,
            user.PasswordHash,
            user.SecurityStamp
        );

        return Result<ApplicationUserDto>.Success(userDto);
    }

    /// <summary>
    /// Maps user from dto.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="userDto">The user dto.</param>
    /// <returns></returns>
    public override Result<ApplicationUser> MapFromDto(ApplicationUser user, ApplicationUserDto userDto)
    {
        if (user is null)
        {
            return Result<ApplicationUser>.Failure(MapperErrors.ModelIsNull());
        }

        user.Id = userDto.UserId;
        user.FirstName = userDto.FirstName;
        user.MiddleName = userDto.MiddleName;
        user.LastName = userDto.LastName;
        user.Email = userDto.Email;
        user.NormalizedEmail = userDto.NormalizedEmail;
        user.EmailConfirmed = userDto.EmailConfirmed;
        user.PasswordHash = userDto.PasswordHash;
        user.SecurityStamp = userDto.SecurityStamp;

        return Result<ApplicationUser>.Success(user);
    }
}