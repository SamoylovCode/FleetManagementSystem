namespace FleetManagementSystemApp.Business.Dtos;

public class VehicleIdentificationDataDto
{
    public Guid VehicleId { get; set; }
    public string? LicencePlate { get; set; }
    public string? Vin { get; set; }
    public DateOnly? YearMade { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}