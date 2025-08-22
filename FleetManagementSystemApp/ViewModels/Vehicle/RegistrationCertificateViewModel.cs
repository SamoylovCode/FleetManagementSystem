using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;
using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Vehicle;

public class RegistrationCertificateViewModel : ISubModel
{
    public Guid RegCertificateId { get; set; }
    public Guid VehicleId { get; set; }
    public string Prefix => "RegistrationCertificate";

    [Display(Name = "Серия, номер")]
    public string? Number { get; set; }

    [Display(Name = "Дата регистрации")]
    [DataType(DataType.Date)]
    public DateOnly? IssueDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}