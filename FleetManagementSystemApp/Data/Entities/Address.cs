namespace FleetManagementSystemApp.Data.Entities;

public class Address
{
    public Guid AddressId { get; set; }
    public string Region { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string House { get; set; }
    public string? Building { get; set; }
    public string? Apartment { get; set; }

    /// <summary>
    /// The foreign key
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Navigation property
    /// </summary>
    public Company Company { get; set; }
}