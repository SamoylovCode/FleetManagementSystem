using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.Data.Entities;

public class RegistrationCertificate
{
    [Key]
    public Guid RegCertificateId { get; set; }
    public string? Number { get; set; }
    public DateOnly? IssueDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;

    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
}