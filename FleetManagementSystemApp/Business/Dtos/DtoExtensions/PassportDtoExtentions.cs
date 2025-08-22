using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

/// <summary>
/// Convertation methods for vehicle passport data to and from DTO model
/// </summary>
public class PassportDtoExtentions : BaseMapper<Passport, PassportDto>
{
    public override Result<PassportDto> ToDto(Passport passport)
    {
        if (passport is null)
        {
            return Result<PassportDto>.Failure(MapperErrors.ModelIsNull());
        }

        var passportDto = new PassportDto
        {
            PassportId = passport.PassportId,
            VehicleId = passport.VehicleId,
            Number = passport.Number,
            IssueDate = passport.IssueDate,
            RowVersion = passport.RowVersion
        };

        return Result<PassportDto>.Success(passportDto);
    }

    public override Result<Passport> MapFromDto(Passport passport, PassportDto passportDto)
    {
        if (passport is null)
        {
            return Result<Passport>.Failure(MapperErrors.ModelIsNull());
        }

        if (passportDto is null)
        {
            return Result<Passport>.Failure(MapperErrors.DtoIsNull());
        }

        passport.Number = passportDto.Number;
        passport.IssueDate = passportDto.IssueDate;
        passport.RowVersion = passportDto.RowVersion;

        return Result<Passport>.Success(passport);
    }
}