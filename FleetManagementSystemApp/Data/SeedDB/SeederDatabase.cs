using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Data.SeedDB;

public class SeederDatabase : ISeederDatabase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeederDatabase(
        ApplicationDbContext dbContext,
        ILogger logger,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        _logger.Information("Starting database seeding...");

        var company = new Company
        {
            CompanyId = Guid.NewGuid(),
            Name = "Test Company",
            PhoneNum = "+71111234567",
            Inn = "1234567890",
            Kpp = "123456789",
            Ogrn = "1234567890123",
            Okpo = "1234567",
            IsMain = true,
        };

        if(_dbContext.Companies.Any(c => c.Name == company.Name))
        {
            _logger.Information("This company already exists.");
            return;
        }
        else
        {
            await _dbContext.Companies.AddAsync(company);
            await _dbContext.SaveChangesAsync();
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "ivanovsky.testcompany@example.com", // email
            Email = "ivanovsky.testcompany@example.com",
            FirstName = "Иван",
            MiddleName = "Иванович",
            LastName = "Ивановский",
            CreatedAt = DateTime.UtcNow,
            CompanyId = company.CompanyId,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, "P@ssw0rd!123");

        if(!createResult.Succeeded)
        {
            _logger.Error("Something went wrong while creating a test user.");
            foreach(var error in createResult.Errors)
            {
                _logger.Error("Error code: {Error code}. Error message: {ErrorMessage}", error.Code, error.Description);
            }
        }

        await _userManager.AddToRoleAsync(user, ApplicationRole.Admin);

        if (!_dbContext.Vehicles.Any(v => v.CompanyId == company.CompanyId))
        {
            var vehicle1 = new Vehicle
            {
                VehicleId = Guid.NewGuid(),
                LicensePlate = "А123АА 01",
                Vin = "F08BCAMXC5JT35232",
                YearMade = new DateOnly(2025, 1, 1),
                IsMain = true,
                RowVersion = Array.Empty<byte>(),
                CompanyId = company.CompanyId
            };
            var vehicle2 = new Vehicle
            {
                VehicleId = Guid.NewGuid(),
                LicensePlate = "А124АА 01",
                Vin = "F08BCAMXC5JT35233",
                YearMade = new DateOnly(2025, 1, 1),
                IsMain = true,
                RowVersion = Array.Empty<byte>(),
                CompanyId = company.CompanyId
            };
            var vehicle3 = new Vehicle
            {
                VehicleId = Guid.NewGuid(),
                LicensePlate = "А125АА 01",
                Vin = "F08BCAMXC5JT35234",
                YearMade = new DateOnly(2025, 1, 1),
                IsMain = true,
                RowVersion = Array.Empty<byte>(),
                CompanyId = company.CompanyId
            };

            await _dbContext.Vehicles.AddRangeAsync(
                new List<Vehicle>
                {
                    vehicle1,
                    vehicle2,
                    vehicle3
                });

            if (!_dbContext.Passports.Any())
            {
                await _dbContext.Passports.AddRangeAsync(
                    new Passport
                    {
                        PassportId = Guid.NewGuid(),
                        Number = "11 АА 123456",
                        IssueDate = new DateOnly(2025, 1, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle1.VehicleId,
                    },
                    new Passport
                    {
                        PassportId = Guid.NewGuid(),
                        Number = "12 АБ 123456",
                        IssueDate = new DateOnly(2025, 1, 2),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle2.VehicleId
                    },
                    new Passport
                    {
                        PassportId = Guid.NewGuid(),
                        Number = "13 АВ 123456",
                        IssueDate = new DateOnly(2025, 1, 3),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle3.VehicleId
                    });
            }

            if (!_dbContext.Insurances.Any())
            {
                await _dbContext.Insurances.AddRangeAsync(
                    new Insurance
                    {
                        InsuranceId = Guid.NewGuid(),
                        Number = "ААА 12345678910",
                        IssuedBy = "ООО Страховая компания",
                        IssueDate = new DateOnly(2025, 6, 1),
                        ExpDate = new DateOnly(2026, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle1.VehicleId
                    },
                    new Insurance
                    {
                        InsuranceId = Guid.NewGuid(),
                        Number = "АБА 12345678910",
                        IssuedBy = "ООО Страховая компания",
                        IssueDate = new DateOnly(2025, 6, 1),
                        ExpDate = new DateOnly(2026, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle2.VehicleId
                    },
                    new Insurance
                    {
                        InsuranceId = Guid.NewGuid(),
                        Number = "АВА 12345678910",
                        IssuedBy = "ООО Страховая компания",
                        IssueDate = new DateOnly(2025, 6, 1),
                        ExpDate = new DateOnly(2026, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle3.VehicleId
                    });
            }

            if (!_dbContext.RegistrationCertificates.Any())
            {
                await _dbContext.RegistrationCertificates.AddRangeAsync(
                    new RegistrationCertificate
                    {
                        RegCertificateId = Guid.NewGuid(),
                        Number = "11 АА 123456",
                        IssueDate = new DateOnly(2025, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle1.VehicleId
                    },
                    new RegistrationCertificate
                    {
                        RegCertificateId = Guid.NewGuid(),
                        Number = "12 АА 123456",
                        IssueDate = new DateOnly(2025, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle2.VehicleId
                    },
                    new RegistrationCertificate
                    {
                        RegCertificateId = Guid.NewGuid(),
                        Number = "13 АА 123456",
                        IssueDate = new DateOnly(2025, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle3.VehicleId
                    });
            }

            if (!_dbContext.CertificateTechInspections.Any())
            {
                await _dbContext.CertificateTechInspections.AddRangeAsync(
                    new CertificateTechInspection
                    {
                        CertificateTechInspectionId = Guid.NewGuid(),
                        CertificateTechInspectionNum = "012345678912345",
                        CertificateTechInspectionIssuedBy = "ООО Техосмотр",
                        CertificateTechInspectionIssueDate = new DateOnly(2025, 6, 1),
                        CertificateTechInspectionExpDate = new DateOnly(2025, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle1.VehicleId
                    },
                    new CertificateTechInspection
                    {
                        CertificateTechInspectionId = Guid.NewGuid(),
                        CertificateTechInspectionNum = "123456789012345",
                        CertificateTechInspectionIssuedBy = "ООО Техосмотр",
                        CertificateTechInspectionIssueDate = new DateOnly(2025, 6, 1),
                        CertificateTechInspectionExpDate = new DateOnly(2025, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle1.VehicleId
                    },
                    new CertificateTechInspection
                    {
                        CertificateTechInspectionId = Guid.NewGuid(),
                        CertificateTechInspectionNum = "234567890123456",
                        CertificateTechInspectionIssuedBy = "ООО Техосмотр",
                        CertificateTechInspectionIssueDate = new DateOnly(2025, 6, 1),
                        CertificateTechInspectionExpDate = new DateOnly(2025, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle1.VehicleId
                    });
            }

            await _dbContext.SaveChangesAsync();
            _logger.Information("Database seeding completed successfully.");
        }
    }
}