using FleetManagementSystemApp.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.Business.Dtos;

public class CertificateTechInspectionDto
{
    public Guid CertificateTechInspectionId { get; set; }
    public string? CertificateTechInspectionNum { get; set; }
    public string? CertificateTechInspectionIssuedBy { get; set; }
    public DateOnly? CertificateTechInspectionIssueDate { get; set; }
    public DateOnly? CertificateTechInspectionExpDate { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public Guid VehicleId { get; set; }
}