using FleetManagementSystemApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Data.SeedDB;

public class SeederDatabase : ISeederDatabase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public SeederDatabase(
        ApplicationDbContext dbContext,
        ILogger logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        _logger.Information("Starting database seeding...");

        //await CreateRolesAsync();

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

        if (_dbContext.Companies.Any(c => c.Name == company.Name))
        {
            _logger.Warning("This company already exists.");
            return;
        }

        var existingRole = await _roleManager.RoleExistsAsync(ApplicationRole.Admin);

        if (!existingRole)
        {
            _logger.Error("Role does not exists.");
            return;
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await _dbContext.Companies.AddAsync(company);
            await _dbContext.SaveChangesAsync();

            if (_dbContext.Vehicles.Any(v => v.CompanyId == company.CompanyId))
            {
                _logger.Warning("These vehicles are already assigned to the company.");
                await transaction.RollbackAsync();
                return;
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "ivanov.testcompany@example.com", // Email
                Email = "ivanov.testcompany@example.com",
                FirstName = "Иван",
                MiddleName = "Иванович",
                LastName = "Иванов",
                CreatedAt = DateTime.UtcNow,
                CompanyId = company.CompanyId,
                EmailConfirmed = true
            };

            var findUserResult = await _userManager.FindByEmailAsync(user.Email);

            if (findUserResult != null)
            {
                _logger.Warning("User with email {Email} already exists.", user.Email);
                await transaction.RollbackAsync();
                return;
            }

            var createUserResult = await _userManager.CreateAsync(user, "P@ssw0rd!123");

            if (!createUserResult.Succeeded)
            {
                _logger.Error("Something went wrong while creating a test user.");
                foreach (var error in createUserResult.Errors)
                {
                    _logger.Error("Error code: {Error code}. Error message: {ErrorMessage}", error.Code, error.Description);
                }
                await transaction.RollbackAsync();
                return;
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, ApplicationRole.Admin);

            if (!addRoleResult.Succeeded)
            {
                _logger.Error("Failed to add user to role");
                foreach (var error in addRoleResult.Errors)
                {
                    _logger.Error("Error: {Code} - {Description}", error.Code, error.Description);
                }
                await transaction.RollbackAsync();
                return;
            }

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

            await _dbContext.SaveChangesAsync();

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
                        Number = "012345678912345",
                        IssuedBy = "ООО Техосмотр",
                        IssueDate = new DateOnly(2025, 6, 1),
                        ExpDate = new DateOnly(2026, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle1.VehicleId
                    },
                    new CertificateTechInspection
                    {
                        CertificateTechInspectionId = Guid.NewGuid(),
                        Number = "123456789012345",
                        IssuedBy = "ООО Техосмотр",
                        IssueDate = new DateOnly(2025, 6, 1),
                        ExpDate = new DateOnly(2026, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle2.VehicleId
                    },
                    new CertificateTechInspection
                    {
                        CertificateTechInspectionId = Guid.NewGuid(),
                        Number = "234567890123456",
                        IssuedBy = "ООО Техосмотр",
                        IssueDate = new DateOnly(2025, 6, 1),
                        ExpDate = new DateOnly(2026, 6, 1),
                        RowVersion = Array.Empty<byte>(),
                        VehicleId = vehicle3.VehicleId
                    });
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.Information("Database seeding completed successfully.");
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occurred while database seeding. Error: {Message}", e.Message);
            await transaction.RollbackAsync();
            throw;
        }
    }

    //public async Task CreateRolesAsync()
    //{
    //    try
    //    {
    //        var roles = new Dictionary<string, string>
    //        {
    //            { "1", ApplicationRole.Admin },
    //            { "2", ApplicationRole.Manager },
    //            { "3", ApplicationRole.Dispatcher },
    //            { "4", ApplicationRole.Inspector }
    //        };

    //        foreach (var role in roles)
    //        {
    //            var roleExists = await _roleManager.RoleExistsAsync(role.Value);
    //            if (!roleExists)
    //            {
    //                await _roleManager.CreateAsync(
    //                    new IdentityRole(role.Value)
    //                    {
    //                        Id = role.Key,
    //                    });
    //            }
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.Error(e, "Failed to create roles.");
    //        throw;
    //    }
    //}
}