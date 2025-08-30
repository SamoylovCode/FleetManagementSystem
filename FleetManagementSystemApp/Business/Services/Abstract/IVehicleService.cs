using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.ViewModels.Vehicle;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IVehicleService
{
    public IQueryable<Vehicle> VehicleQueryWithAll();
    public VehiclePageViewModel GetNewVehiclePage(Guid? id = null);
    public Task<Result<Vehicle>> GetVehicleByIdAsync(Guid vehicleId);
    public Task<Result<List<VehicleDto>>> GetAllVehiclesAsync();
    public Task<Result> CreateVehicleAsync(VehiclePageViewModel viewModel);
    public Task<Result> RemoveVehicleAsync(Vehicle vehicle);
}