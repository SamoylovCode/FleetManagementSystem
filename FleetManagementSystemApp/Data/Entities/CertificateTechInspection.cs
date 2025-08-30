using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.Data.Entities;

public class CertificateTechInspection
{
    public Guid CertificateTechInspectionId { get; set; }
    public string? CertificateTechInspectionNum { get; set; }
    public string? CertificateTechInspectionIssuedBy { get; set; }
    public DateOnly? CertificateTechInspectionIssueDate { get; set; }
    public DateOnly? CertificateTechInspectionExpDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
}