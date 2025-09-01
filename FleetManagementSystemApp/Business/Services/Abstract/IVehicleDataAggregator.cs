using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Common;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IVehicleDataAggregator
{
    public Task<Result<VehicleDataDto>> GetAsync(Guid vehicleId);
}