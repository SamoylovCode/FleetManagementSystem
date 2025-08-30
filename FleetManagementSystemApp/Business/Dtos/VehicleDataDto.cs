namespace FleetManagementSystemApp.Business.Dtos;

public class VehicleDataDto
{
    public VehicleDto Vehicle { get; set; }
    public InsuranceDto Insurance { get; set; }
    public PassportDto Passport { get; set; }
    public RegistrationCertificateDto Registration {  get; set; }
    public CertificateTechInspectionDto CertificateTechInspection { get; set; }

    public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
}