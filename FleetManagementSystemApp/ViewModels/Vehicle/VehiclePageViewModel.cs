using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;

namespace FleetManagementSystemApp.ViewModels.Vehicle;

public class VehiclePageViewModel : IAggregateViewModel
{
    public Guid VehicleId { get; set; }
    public Guid GetEntityId() => VehicleId;
    public VehicleIdentificationDataViewModel VehicleIdentificationData { get; set; }
    public PassportViewModel Passport { get; set; }
    public InsuranceViewModel Insurance { get; set; }
    public RegistrationCertificateViewModel RegistrationCertificate { get; set; }
}