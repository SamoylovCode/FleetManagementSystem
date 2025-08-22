using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

public class VehicleDtoExtentions : BaseMapper<Vehicle, VehicleDto>
{
    public override Result<VehicleDto> ToDto(Vehicle vehicle)
    {
        if (vehicle is null)
        {
            return Result<VehicleDto>.Failure(MapperErrors.ModelIsNull());
        }

        var vehicleDto = new VehicleDto()
        {
            CompanyId = vehicle.CompanyId,
            VehicleId = vehicle.VehicleId,
            LicensePlate = vehicle.LicensePlate,
            Vin = vehicle.Vin,
            YearMade = vehicle.YearMade,
            IsMain = vehicle.IsMain,
            RowVersion = vehicle.RowVersion
        };

        return Result<VehicleDto>.Success(vehicleDto);
    }

    public override Result<Vehicle> MapFromDto(Vehicle vehicle, VehicleDto vehicleDto)
    {
        if (vehicle is null)
        {
            return Result<Vehicle>.Failure(MapperErrors.ModelIsNull());
        }

        if (vehicleDto is null)
        {
            return Result<Vehicle>.Failure(MapperErrors.DtoIsNull());
        }

        vehicle.LicensePlate = vehicleDto.LicensePlate;
        vehicle.Vin = vehicleDto.Vin;
        vehicle.YearMade = vehicleDto.YearMade;
        vehicle.IsMain = vehicleDto.IsMain;
        vehicle.RowVersion = vehicleDto.RowVersion;

        return Result<Vehicle>.Success(vehicle);
    }
}
