namespace FleetManagementSystemApp.Business.Dtos;

/// <summary>
/// Represents company data transfer object (DTO)
/// </summary>
public class CompanyDto
{
    public Guid CompanyId { get; init; }
    public string CompanyName { get; init; }
    public string PhoneNum { get; init; }
    public string Inn { get; init; }
    public string Kpp { get; init; }
    public string Ogrn { get; init; }
    public string Okpo { get; init; }
    public bool IsMain { get; init; }

    public CompanyDto(
        Guid companyId,
        string companyName,
        string phoneNum,
        string inn,
        string kpp,
        string ogrn,
        string okpo,
        bool isMain)
    {
        CompanyId = companyId;
        CompanyName = companyName;
        PhoneNum = phoneNum;
        Inn = inn;
        Kpp = kpp;
        Ogrn = ogrn;
        Okpo = okpo;
        IsMain = isMain;
    }
}