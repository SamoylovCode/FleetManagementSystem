using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.Data.Entities;

public class Vehicle
{
    public Guid VehicleId { get; set; }
    public string? LicensePlate { get; set; }
    public string? Vin { get; set; }
    public DateOnly? YearMade { get; set; }
    public bool IsMain { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;

    public Guid CompanyId { get; set; }

    public Company Company { get; set; }
    public Insurance Insurance { get; set; }
    public Passport Passport { get; set; }
    public RegistrationCertificate RegCertificate { get; set; }
}