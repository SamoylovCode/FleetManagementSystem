namespace FleetManagementSystemApp.Data.Entities;

public class Company
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string PhoneNum { get; set; }
    public string Inn { get; set; }
    public string? Kpp { get; set; }
    public string? Ogrn { get; set; }
    public string? Okpo { get; set; }

    /// <summary>
    /// True - the company is an app user's company;
    /// False - the company's data is used by the business logic.
    /// </summary>
    public bool IsMain { get; set; }

    /// <summary>
    /// The foreign key
    /// </summary>
    /// <value>
    /// The users.
    /// </value>
    public IEnumerable<ApplicationUser>? Users { get; set; }

    /// <summary>
    /// The navigation property
    /// </summary>
    public Address? Address { get; set; }
}