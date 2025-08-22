using FleetManagementSystemApp.Infrastructure.ModelBinders.Attributes;
using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;
using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Vehicle;

public class InsuranceViewModel : ISubModel
{
    public Guid InsuranceId { get; set; }
    public Guid VehicleId { get; set; }
    public string Prefix => "Insurance";

    [Display(Name = "Серия, номер")]
    public string? Number { get; set; }

    [Display(Name = "Кем выдан")]
    public string? IssuedBy { get; set; }
    public DateOnly? IssueDate { get; set; }
    public DateOnly? ExpDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    [Display(Name = "Срок действия полиса")]
    [DateRangeBinder(nameof(IssueDate), nameof(ExpDate))]
    public string? PeriodString { get; set; } //только во view model
}