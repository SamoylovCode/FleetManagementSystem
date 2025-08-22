namespace FleetManagementSystemApp.Business.Dtos;

/// <summary>
/// Represents a postal address data transfer object (DTO).
/// </summary>
public class AddressDto
{
    public Guid AddressId { get; init; }
    public string Region { get; init; }
    public string City { get; init; }
    public string Street { get; init; }
    public string House { get; init; }
    public string? Building { get; init; }
    public string? Apartment { get; init; }

    public AddressDto(
        Guid addressId,
        string region,
        string city,
        string street,
        string house,
        string? building,
        string? apartment)
    {
        AddressId = addressId;
        Region = region;
        City = city;
        Street = street;
        House = house;
        Building = building;
        Apartment = apartment;
    }
}