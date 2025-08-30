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
            CertificateTechInspectionNum = certTechInspection.CertificateTechInspectionNum,
            CertificateTechInspectionIssuedBy = certTechInspection.CertificateTechInspectionIssuedBy,
            CertificateTechInspectionIssueDate = certTechInspection.CertificateTechInspectionIssueDate,
            CertificateTechInspectionExpDate = certTechInspection.CertificateTechInspectionExpDate,
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
        certTechInspection.CertificateTechInspectionNum = certTechInspectionDto.CertificateTechInspectionNum;
        certTechInspection.CertificateTechInspectionIssuedBy = certTechInspectionDto.CertificateTechInspectionIssuedBy;
        certTechInspection.CertificateTechInspectionIssueDate = certTechInspectionDto.CertificateTechInspectionIssueDate;
        certTechInspection.CertificateTechInspectionExpDate = certTechInspectionDto.CertificateTechInspectionExpDate;
        certTechInspection.RowVersion = certTechInspectionDto.RowVersion;

        return Result<CertificateTechInspection>.Success(certTechInspection);
    }
}