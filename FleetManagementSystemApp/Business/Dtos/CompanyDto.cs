namespace FleetManagementSystemApp.Business.Dtos;

/// <summary>
/// Represents company data transfer object (DTO)
/// </summary>
/// <param name="companyId">Unique company identifier</param>
/// <param name="companyName">Official company name</param>
/// <param name="phoneNum">Contact phone number</param>
/// <param name="inn">Taxpayer Identification Number (ИНН)</param>
/// <param name="kpp">Tax Registration Reason Code (КПП)</param>
/// <param name="ogrn">Primary State Registration Number (ОГРН)</param>
/// <param name="okpo">Russian Classification of Enterprises code (ОКПО)</param>
/// <param name="isMain">Indicates if this is the main company</param>
public class CompanyDto(Guid companyId,
    string companyName,
    string phoneNum,
    string inn,
    string kpp,
    string ogrn,
    string okpo,
    bool isMain)
{
    /// <summary>
    /// Gets the company identifier.
    /// </summary>
    /// <value>
    /// The company identifier.
    /// </value>
    public Guid CompanyId { get; init; } = companyId;

    /// <summary>
    /// Gets the name of the company.
    /// </summary>
    /// <value>
    /// The name of the company.
    /// </value>
    public string CompanyName { get; init; } = companyName;

    /// <summary>
    /// Gets the phone number.
    /// </summary>
    /// <value>
    /// The phone number.
    /// </value>
    public string PhoneNum { get; init; } = phoneNum;

    /// <summary>
    /// Gets the inn.
    /// </summary>
    /// <value>
    /// The inn.
    /// </value>
    public string Inn { get; init; } = inn;

    /// <summary>
    /// Gets the KPP.
    /// </summary>
    /// <value>
    /// The KPP.
    /// </value>
    public string Kpp { get; init; } = kpp;

    /// <summary>
    /// Gets the ogrn.
    /// </summary>
    /// <value>
    /// The ogrn.
    /// </value>
    public string Ogrn { get; init; } = ogrn;

    /// <summary>
    /// Gets the okpo.
    /// </summary>
    /// <value>
    /// The okpo.
    /// </value>
    public string Okpo { get; init; } = okpo;

    /// <summary>
    /// Gets a value indicating whether this instance is main.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is main; otherwise, <c>false</c>.
    /// </value>
    public bool IsMain { get; init; } = isMain;
}