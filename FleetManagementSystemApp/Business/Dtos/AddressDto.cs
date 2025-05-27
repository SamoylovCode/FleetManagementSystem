namespace FleetManagementSystemApp.Business.Dtos;

/// <summary>
/// Represents a postal address data transfer object (DTO).
/// </summary>
/// <param name="addressId">The unique identifier of the address.</param>
/// <param name="region">The region or state part of the address.</param>
/// <param name="city">The city or locality part of the address.</param>
/// <param name="street">The street name.</param>
/// <param name="house">The house or building number.</param>
/// <param name="building">Optional. The building or block number (if applicable).</param>
/// <param name="apartment">Optional. The apartment or unit number (if applicable).</param>
public class AddressDto(Guid addressId,
    string region,
    string city,
    string street,
    string house,
    string? building,
    string? apartment)
{
    /// <summary>
    /// Gets the address identifier.
    /// </summary>
    /// <value>
    /// The address identifier.
    /// </value>
    public Guid AddressId { get; init; } = addressId;
    /// <summary>
    /// Gets the region.
    /// </summary>
    /// <value>
    /// The region.
    /// </value>
    public string Region { get; init; } = region;
    /// <summary>
    /// Gets the city.
    /// </summary>
    /// <value>
    /// The city.
    /// </value>
    public string City { get; init; } = city;
    /// <summary>
    /// Gets the street.
    /// </summary>
    /// <value>
    /// The street.
    /// </value>
    public string Street { get; init; } = street;
    /// <summary>
    /// Gets the house.
    /// </summary>
    /// <value>
    /// The house.
    /// </value>
    public string House { get; init; } = house;
    /// <summary>
    /// Gets the building.
    /// </summary>
    /// <value>
    /// The building.
    /// </value>
    public string? Building { get; init; } = building;
    /// <summary>
    /// Gets the apartment.
    /// </summary>
    /// <value>
    /// The apartment.
    /// </value>
    public string? Apartment { get; init; } = apartment;
}