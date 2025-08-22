using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.Data.Entities;

public class Insurance
{
    public Guid InsuranceId { get; set; }
    public string? Number { get; set; }
    public string? IssuedBy { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;

    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
}