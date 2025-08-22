namespace FleetManagementSystemApp.Business.Dtos;

public class InsuranceDto
{
    public Guid InsuranceId { get; set; }
    public string? Number { get; set; }
    public string? IssuedBy { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Guid VehicleId { get; set; }
}