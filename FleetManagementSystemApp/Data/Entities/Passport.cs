using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.Data.Entities;

public class Passport
{
    public Guid PassportId { get; set; }
    public string? Number { get; set; }
    public DateOnly? IssueDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;

    public Guid VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
}