using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

/// <summary>
/// Convertation methods for Address data to and from DTO model
/// </summary>
public class AddressDtoExtentions : BaseMapper<Address, AddressDto>
{
    /// <summary>
    /// Converts address to dto.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <returns></returns>
    public override Result<AddressDto> ToDto(Address address)
    {
        var addressDto = new AddressDto
        (
            address.AddressId,
            address.Region,
            address.City,
            address.Street,
            address.House,
            address.Building,
            address.Apartment
        );

        return Result<AddressDto>.Success(addressDto);
    }
    /// <summary>
    /// Maps address from dto.
    /// </summary>
    /// <param name="address">The address.</param>
    /// <param name="addressDto">The address dto.</param>
    /// <returns></returns>
    public override Result<Address> MapFromDto(Address address, AddressDto addressDto)
    {
        address.AddressId = addressDto.AddressId;
        address.Region = addressDto.Region;
        address.City = addressDto.City;
        address.Street = addressDto.Street;
        address.House = addressDto.House;
        address.Building = addressDto.Building ?? string.Empty;
        address.Apartment = addressDto.Apartment ?? string.Empty;

        return Result<Address>.Success(address);
    }
}