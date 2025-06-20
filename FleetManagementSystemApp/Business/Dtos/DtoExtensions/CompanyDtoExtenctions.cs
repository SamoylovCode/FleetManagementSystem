using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

/// <summary>
/// Convertation methods for Company data to and from DTO model
/// </summary>
public class CompanyDtoExtenctions : BaseMapper<Company, CompanyDto>
{
    /// <summary>
    /// Converts company to dto.
    /// </summary>
    /// <param name="company">The company.</param>
    /// <returns></returns>
    public override Result<CompanyDto> ToDto(Company company)
    {
        if(company is null)
        {
            return Result<CompanyDto>.Failure(MapperErrors.ModelIsNull());
        }

        var companyDto = new CompanyDto
        (
            company.CompanyId,
            company.CompanyName,
            company.PhoneNum,
            company.Inn,
            company.Kpp,
            company.Ogrn,
            company.Okpo,
            company.IsMain
        );

        return Result<CompanyDto>.Success(companyDto);
    }

    /// <summary>
    /// Maps company from dto.
    /// </summary>
    /// <param name="company">The company.</param>
    /// <param name="companyDto">The company dto.</param>
    /// <returns></returns>
    public override Result<Company> MapFromDto(Company company, CompanyDto companyDto)
    {
        company.CompanyId = companyDto.CompanyId;
        company.CompanyName = companyDto.CompanyName;
        company.PhoneNum = companyDto.PhoneNum;
        company.Inn = companyDto.Inn;
        company.Kpp = companyDto.Kpp;
        company.Ogrn = companyDto.Ogrn;
        company.Okpo = companyDto.Okpo;
        company.IsMain = companyDto.IsMain;

        return Result<Company>.Success(company);
    }
}