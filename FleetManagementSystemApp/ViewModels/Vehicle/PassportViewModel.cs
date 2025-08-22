using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;
using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Vehicle;

public class PassportViewModel : ISubModel
{
    public Guid PassportId { get; set; }
    public Guid VehicleId { get; set; }
    public string Prefix => "Passport";

    [Display(Name = "Серия, номер")]
    public string? Number { get; set; }

    [Display(Name = "Дата выдачи")]
    [DataType(DataType.Date)]
    public DateOnly? IssueDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}