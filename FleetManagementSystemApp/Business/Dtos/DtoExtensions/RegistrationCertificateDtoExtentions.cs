using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

/// <summary>
/// Convertation methods for vehicle registration certificate data to and from DTO model
/// </summary>
public class RegistrationCertificateDtoExtentions : BaseMapper<RegistrationCertificate, RegistrationCertificateDto>
{
    public override Result<RegistrationCertificateDto> ToDto(RegistrationCertificate regCertificate)
    {
        if (regCertificate is null)
        {
            return Result<RegistrationCertificateDto>.Failure(MapperErrors.ModelIsNull());
        }

        var registrationCertificate = new RegistrationCertificateDto
        {
            RegCertificateId = regCertificate.RegCertificateId,
            VehicleId = regCertificate.VehicleId,
            Number = regCertificate.Number,
            IssueDate = regCertificate.IssueDate,
            RowVersion = regCertificate.RowVersion
        };

        return Result<RegistrationCertificateDto>.Success(registrationCertificate);
    }

    public override Result<RegistrationCertificate> MapFromDto(RegistrationCertificate regCertificate, RegistrationCertificateDto regCertificateDto)
    {
        if (regCertificate is null)
        {
            return Result<RegistrationCertificate>.Failure(MapperErrors.ModelIsNull());
        }

        if (regCertificateDto is null)
        {
            return Result<RegistrationCertificate>.Failure(MapperErrors.DtoIsNull());
        }

        regCertificate.VehicleId = regCertificateDto.VehicleId;
        regCertificate.Number = regCertificateDto.Number;
        regCertificate.IssueDate = regCertificateDto.IssueDate;
        regCertificate.RowVersion = regCertificateDto.RowVersion;

        return Result<RegistrationCertificate>.Success(regCertificate);
    }
}