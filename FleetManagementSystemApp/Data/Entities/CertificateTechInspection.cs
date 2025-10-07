using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.Data.Entities;

public class CertificateTechInspection
{
    public Guid CertificateTechInspectionId { get; set; }
    public string? Number { get; set; }
    public string? IssuedBy { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
}