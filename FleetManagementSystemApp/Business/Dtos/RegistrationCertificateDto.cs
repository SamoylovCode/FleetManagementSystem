namespace FleetManagementSystemApp.Business.Dtos;

public class RegistrationCertificateDto
{
    public Guid RegCertificateId { get; set; }
    public string? Number { get; set; }
    public DateOnly? IssueDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Guid VehicleId { get; set; }
}