namespace FleetManagementSystemApp.Business.Dtos;

public class VehicleTypeApprovalDto
{
    public Guid ApprovalId { get; set; }
    public string Number { get; set; }
    public DateTime IssueDate { get; set; }
}