using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

/// <summary>
/// Convertation methods for vehicle identification data to and from DTO model
/// </summary>
public class VehicleIdentificationDataDtoExtentions
{
    public Result<VehicleIdentificationDataDto> ToDto(Vehicle vehicle)
    {
        if (vehicle is null)
        {
            return Result<VehicleIdentificationDataDto>.Failure(MapperErrors.ModelIsNull());
        }

        var vehicleIdentificationDataDto = new VehicleIdentificationDataDto
        {
            VehicleId = vehicle.VehicleId,
            LicencePlate = vehicle.LicensePlate,
            Vin = vehicle.Vin,
            YearMade = vehicle.YearMade,
            RowVersion = vehicle.RowVersion
        };

        return Result<VehicleIdentificationDataDto>.Success(vehicleIdentificationDataDto);
    }

    public Result<Vehicle> MapFromDto(Vehicle vehicle, VehicleIdentificationDataDto vehicleIdentificationDataDto)
    {
        if (vehicle is null)
        {
            return Result<Vehicle>.Failure(MapperErrors.ModelIsNull());
        }

        if (vehicleIdentificationDataDto is null)
        {
            return Result<Vehicle>.Failure(MapperErrors.DtoIsNull());
        }

        vehicle.VehicleId = vehicleIdentificationDataDto.VehicleId;
        vehicle.LicensePlate = vehicleIdentificationDataDto.LicencePlate;
        vehicle.Vin = vehicleIdentificationDataDto.Vin;
        vehicle.YearMade = vehicleIdentificationDataDto.YearMade;
        vehicle.RowVersion = vehicleIdentificationDataDto.RowVersion;

        return Result<Vehicle>.Success(vehicle);
    }
}