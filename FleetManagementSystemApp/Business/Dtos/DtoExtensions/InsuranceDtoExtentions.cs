using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

/// <summary>
/// Convertation methods for vehicle insurance policy data to and from DTO model
/// </summary>
public class InsuranceDtoExtentions : BaseMapper<Insurance, InsuranceDto>
{
    public override Result<InsuranceDto> ToDto(Insurance insurance)
    {
        if (insurance is null)
        {
            return Result<InsuranceDto>.Failure(MapperErrors.ModelIsNull());
        }

        var insuranceDto = new InsuranceDto
        {
            InsuranceId = insurance.InsuranceId,
            VehicleId = insurance.VehicleId,
            Number = insurance.Number,
            IssuedBy = insurance.IssuedBy,
            IssueDate = insurance.IssueDate,
            ExpDate = insurance.ExpDate,
            RowVersion = insurance.RowVersion
        };

        return Result<InsuranceDto>.Success(insuranceDto);
    }

    public override Result<Insurance> MapFromDto(Insurance insurance, InsuranceDto insuranceDto)
    {
        if (insurance is null)
        {
            return Result<Insurance>.Failure(MapperErrors.ModelIsNull());
        }

        if (insuranceDto is null)
        {
            return Result<Insurance>.Failure(MapperErrors.DtoIsNull());
        }

        insurance.VehicleId = insuranceDto.VehicleId;
        insurance.Number = insuranceDto.Number;
        insurance.IssuedBy = insuranceDto.IssuedBy;
        insurance.IssueDate = insuranceDto.IssueDate;
        insurance.ExpDate = insuranceDto.ExpDate;

        return Result<Insurance>.Success(insurance);
    }
}