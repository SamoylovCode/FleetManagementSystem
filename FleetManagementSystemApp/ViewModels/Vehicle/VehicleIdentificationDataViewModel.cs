using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;
using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.ViewModels.Vehicle;

public class VehicleIdentificationDataViewModel : ISubModel
{
    public Guid VehicleId { get; set; }
    public string Prefix => "VehicleIdentificationData";

    [Display(Name ="ГРЗ ТС")]
    public string? LicencePlate { get; set; }

    [Display(Name ="Идентификационный номер (VIN)")]
    public string? Vin { get; set; }

    [Display(Name ="Дата изготовления")]
    [DataType(DataType.Date)]
    public DateOnly? YearMade { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}