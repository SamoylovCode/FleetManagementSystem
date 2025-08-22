namespace FleetManagementSystemApp.ViewModels.Vehicle;

public class VehicleRemoveViewModel
{
    public Guid VehicleId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}