using FleetManagementSystemApp.Common;

namespace FleetManagementSystemApp.Business.Services.Errors;

public static class VehicleServiceErrors
{
    public static Error CreatingVehicleFailed() =>
        new Error(
            VehicleServiceErrorCodes.CreatingVehicleFailed,
            userDesc: "Возникла ошибка при создании транспортного средства.",
            devDesc: "An error occurred while creating the vehicle.");

    public static Error VehicleNotFound(string vehicleId) =>
        new Error(
            VehicleServiceErrorCodes.VehicleNotFound,
            userDesc: "Транспортное средство не найдено.",
            devDesc: $"The vehicle '{vehicleId}' not found.",
            context: new { VehicleId = vehicleId });

    public static Error FailedToRemoveVehicle(string vehicleId) =>
        new Error(
            VehicleServiceErrorCodes.FailedToRemoveVehicle,
            userDesc: "Не удалось удалить транспортное средство.",
            devDesc: $"Failed to remove vehicle with ID '{vehicleId}'.",
            context: new { VehicleId = vehicleId });
}