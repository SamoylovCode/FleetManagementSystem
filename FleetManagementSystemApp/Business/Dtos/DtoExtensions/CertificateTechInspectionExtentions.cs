using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

public class CertificateTechInspectionExtentions : BaseMapper<CertificateTechInspection, CertificateTechInspectionDto>
{
    public override Result<CertificateTechInspectionDto> ToDto(CertificateTechInspection certTechInspection)
    {
        if (certTechInspection is null)
        {
            return Result<CertificateTechInspectionDto>.Failure(MapperErrors.ModelIsNull());
        }

        var certTechInspectionDto = new CertificateTechInspectionDto
        {
            CertificateTechInspectionId = certTechInspection.CertificateTechInspectionId,
            VehicleId = certTechInspection.VehicleId,
            Number = certTechInspection.Number,
            IssuedBy = certTechInspection.IssuedBy,
            IssueDate = certTechInspection.IssueDate,
            ExpDate = certTechInspection.ExpDate,
            RowVersion = certTechInspection.RowVersion
        };

        return Result<CertificateTechInspectionDto>.Success(certTechInspectionDto);
    }

    public override Result<CertificateTechInspection> MapFromDto(CertificateTechInspection certTechInspection, CertificateTechInspectionDto certTechInspectionDto)
    {
        if (certTechInspection is null)
        {
            return Result<CertificateTechInspection>.Failure(MapperErrors.ModelIsNull());
        }

        if (certTechInspectionDto is null)
        {
            return Result<CertificateTechInspection>.Failure(MapperErrors.DtoIsNull());
        }

        certTechInspection.CertificateTechInspectionId = certTechInspectionDto.CertificateTechInspectionId;
        certTechInspection.VehicleId = certTechInspectionDto.VehicleId;
        certTechInspection.Number = certTechInspectionDto.Number;
        certTechInspection.IssuedBy = certTechInspectionDto.IssuedBy;
        certTechInspection.IssueDate = certTechInspectionDto.IssueDate;
        certTechInspection.ExpDate = certTechInspectionDto.ExpDate;
        certTechInspection.RowVersion = certTechInspectionDto.RowVersion;

        return Result<CertificateTechInspection>.Success(certTechInspection);
    }
}