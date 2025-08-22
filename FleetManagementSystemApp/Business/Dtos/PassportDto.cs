namespace FleetManagementSystemApp.Business.Dtos;

public class PassportDto
{
    public Guid PassportId { get; set; }
    public string? Number { get; set; }
    public DateOnly? IssueDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Guid VehicleId { get; set; }
}