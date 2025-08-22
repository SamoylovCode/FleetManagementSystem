namespace FleetManagementSystemApp.Business.Dtos;

public class VehicleDto
{
    public Guid VehicleId { get; set; }
    public string? LicensePlate { get; set; }
    public string? Vin { get; set; }
    public DateOnly? YearMade { get; set; }
    public bool IsMain { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Guid CompanyId { get; set; }
}