using Microsoft.AspNetCore.Identity;

namespace FleetManagementSystemApp.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; }
}