namespace FleetManagementSystemApp.Data.Entities;

public class VehicleTypeApploval
{
    public Guid ApprovalId { get; set; }
    public string Number { get; set; }
    public DateTime IssueDate { get; set; }
}