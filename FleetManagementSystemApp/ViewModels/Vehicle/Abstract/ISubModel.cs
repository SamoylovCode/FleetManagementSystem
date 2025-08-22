namespace FleetManagementSystemApp.ViewModels.Vehicle.Abstract;

public interface ISubModel
{
    Guid VehicleId { get; set; }
    string Prefix { get; }
}