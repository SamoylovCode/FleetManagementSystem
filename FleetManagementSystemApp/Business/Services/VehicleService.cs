using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Infrastructure.Caching;
using FleetManagementSystemApp.Validators;
using FleetManagementSystemApp.ViewModels.Vehicle;
using ILogger = Serilog.ILogger;
using Microsoft.EntityFrameworkCore;
using static FleetManagementSystemApp.Common.Extensions.Levels;
using System.Data;

namespace FleetManagementSystemApp.Business.Services;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IHybridCache _hybridCache;
    private readonly ICurrentUserService _currentUserService;
    private readonly VehicleDtoExtentions _vehicleMapper;
    private readonly DateRangeParser _dateRangeParser;

    public VehicleService(
        ApplicationDbContext dbContext,
        ILogger logger,
        IHybridCache hybridCache,
        ICurrentUserService currentUserService,
        VehicleDtoExtentions vehicleMapper,
        DateRangeParser dateRangeParser)
    {
        _dbContext = dbContext;
        _logger = logger;
        _hybridCache = hybridCache;
        _currentUserService = currentUserService;
        _vehicleMapper = vehicleMapper;
        _dateRangeParser = dateRangeParser;
    }

    public IQueryable<Vehicle> VehicleQueryWithAll()
    {
        return _dbContext.Vehicles
            .Include(v => v.Passport)
            .Include(v => v.Insurance)
            .Include(v => v.RegCertificate)
            .Include(v => v.CertificateTechInspection);
    }

    public VehiclePageViewModel GetNewVehiclePage(Guid? id = null)
    {
        Guid vehicleId;

        if (id == Guid.Empty || id == null)
        {
            _logger.Log(CommonErrors.ParamIsNullOrEmpty(typeof(VehicleService)), Info);
            vehicleId = Guid.NewGuid();
        }
        else
        {
            vehicleId = id.Value;
        }

        var vehiclePageVm = new VehiclePageViewModel
        {
            VehicleId = vehicleId,
            VehicleIdentificationData = new VehicleIdentificationDataViewModel
            {
                VehicleId = vehicleId,
                YearMade = DateOnly.FromDateTime(DateTime.UtcNow),
                Vin = default!
            },
            Passport = new PassportViewModel
            {
                VehicleId = vehicleId,
                PassportId = Guid.NewGuid(),
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow)
            },
            Insurance = new InsuranceViewModel
            {
                VehicleId = vehicleId,
                InsuranceId = Guid.NewGuid(),
                ExpDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow)
            },
            RegistrationCertificate = new RegistrationCertificateViewModel
            {
                VehicleId = vehicleId,
                RegCertificateId = Guid.NewGuid(),
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow)
            },
            CertificateTechInspection = new CertificateTechInspectionViewModel
            {
                VehicleId = vehicleId,
                CertificateTechInspectionId = Guid.NewGuid(),
                IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpDate = DateOnly.FromDateTime(DateTime.UtcNow)
            }
        };

        return vehiclePageVm;
    }

    public async Task<Result<List<VehicleDto>>> GetAllVehiclesAsync()
    {
        var companyId = _currentUserService.CompanyGuid;
        _logger.Information("Getting list of company {CompanyId} vehicles.", companyId);
        if (companyId == Guid.Empty)
        {
            _logger.Log(UserServiceErrors.CompanyNotFound(companyId.ToString()), Warning);
            return Result<List<VehicleDto>>.Failure(UserServiceErrors.CompanyNotFound(null));
        }

        var cachedDtoList = await _hybridCache.GetOrAddAsync(async () =>
        {
            var vehicles = await VehicleQueryWithAll()
                .Where(v => v.CompanyId == companyId)
                .AsNoTracking()
                .ToListAsync();

            if (vehicles.Count > 0)
            {
                _logger.Information("Returned list of {VehiclesCount} company {CompanyId} vehicles.", vehicles.Count, companyId);
            }
            else
            {
                _logger.Information("Company {CopmanyId} has no vehicles.", companyId);
            }

            return _vehicleMapper.ToDto(vehicles).Value ?? new List<VehicleDto>();
        },
        key: companyId.ToString(),
        ttl: TimeSpan.FromMinutes(2),
        prefix: CachePrefixes.VehiclesList);

        return Result<List<VehicleDto>>.Success(cachedDtoList);
    }

    public async Task<Result<Vehicle>> GetVehicleByIdAsync(Guid vehicleId)
    {
        if (vehicleId == Guid.Empty)
        {
            _logger.Log(CommonErrors.ParamIsNullOrEmpty(typeof(VehicleService)), Levels.Error);
            return Result<Vehicle>.Failure(CommonErrors.ParamIsNullOrEmpty(typeof(VehicleService)));
        }

        var vehicle = await VehicleQueryWithAll()
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

        if (vehicle is null)
        {
            _logger.Log(VehicleServiceErrors.VehicleNotFound(vehicleId.ToString()), Info);
            return Result<Vehicle>.Failure(VehicleServiceErrors.VehicleNotFound(vehicleId.ToString()));
        }

        return Result<Vehicle>.Success(vehicle);
    }

    public async Task<Result> CreateVehicleAsync(VehiclePageViewModel viewModel)
    {
        if (viewModel == null)
        {
            _logger.Log(CommonErrors.ParamIsNullOrEmpty(typeof(VehicleService)), Levels.Error);
            return Result.Failure(CommonErrors.ParamIsNullOrEmpty(typeof(VehicleService)));
        }

        _ = _dateRangeParser.TryParse(viewModel.Insurance.PeriodString, out DateOnly? startInsuranceDate, out DateOnly? endInsuranceDate);
        _ = _dateRangeParser.TryParse(viewModel.CertificateTechInspection.PeriodString, out DateOnly? startCertificationDate, out DateOnly? endCertificationDate);

        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var vehicle = new Vehicle
                {
                    VehicleId = (viewModel.VehicleId == Guid.Empty)
                                    ? Guid.NewGuid()
                                    : viewModel.VehicleId,
                    CompanyId = _currentUserService.CompanyGuid,
                    LicensePlate = viewModel.VehicleIdentificationData.LicencePlate,
                    Vin = viewModel.VehicleIdentificationData.Vin,
                    YearMade = viewModel.VehicleIdentificationData.YearMade,
                    IsMain = true,
                };

                await _dbContext.AddAsync(vehicle);
                await _dbContext.SaveChangesAsync();

                var insurance = new Insurance
                {
                    VehicleId = vehicle.VehicleId,
                    InsuranceId = viewModel.Insurance.InsuranceId,
                    Number = viewModel.Insurance.Number,
                    IssuedBy = viewModel.Insurance.IssuedBy,
                    IssueDate = startInsuranceDate,
                    ExpDate = endInsuranceDate
                };

                var passport = new Passport
                {
                    VehicleId = vehicle.VehicleId,
                    PassportId = viewModel.Passport.PassportId,
                    Number = viewModel.Passport.Number,
                    IssueDate = viewModel.Passport.IssueDate
                };

                var regCertificate = new RegistrationCertificate
                {
                    VehicleId = vehicle.VehicleId,
                    RegCertificateId = viewModel.RegistrationCertificate.RegCertificateId,
                    Number = viewModel.RegistrationCertificate.Number,
                    IssueDate = viewModel.RegistrationCertificate.IssueDate
                };

                var certTechInspection = new CertificateTechInspection
                {
                    VehicleId= vehicle.VehicleId,
                    CertificateTechInspectionId = viewModel.CertificateTechInspection.CertificateTechInspectionId,
                    Number = viewModel.CertificateTechInspection.Number,
                    IssuedBy = viewModel.CertificateTechInspection.IssuedBy,
                    IssueDate = startCertificationDate,
                    ExpDate = endCertificationDate
                };

                var entities = new List<object>()
                {
                    insurance,
                    passport,
                    regCertificate,
                    certTechInspection
                };

                await _dbContext.AddRangeAsync(entities);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                await _hybridCache.RemoveByPrefixAsync(CachePrefixes.VehicleAggregateFull(vehicle.VehicleId));
                await _hybridCache.RemoveByPrefixAsync(CachePrefixes.VehiclesList);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Creating a vehicle failed. Error: {Error}", e.Message);

                await transaction.RollbackAsync();
                return Result.Failure(VehicleServiceErrors.CreatingVehicleFailed());
            }
        }

        return Result.Success();
    }

    public async Task<Result> RemoveVehicleAsync(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            _logger.Log(CommonErrors.ParamIsNullOrEmpty(typeof(VehicleService)));
            return Result.Failure(CommonErrors.ParamIsNullOrEmpty(typeof(VehicleService)));
        }

        var vehicleData = await VehicleQueryWithAll()
            .FirstOrDefaultAsync(v => v.VehicleId  == vehicle.VehicleId);

        if (vehicleData == null)
        {
            _logger.Log(AggregateModelServiceErrors.AggregateSubModelsNotFound(vehicle.VehicleId.ToString()), Info);
            return Result.Failure(AggregateModelServiceErrors.AggregateSubModelsNotFound(vehicle.VehicleId.ToString()));
        }

        _dbContext.Entry(vehicleData).OriginalValues["RowVersion"] = vehicle.RowVersion;

        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                _dbContext.Vehicles.Remove(vehicleData);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _hybridCache.RemoveByPrefixAsync(CachePrefixes.VehicleAggregateFull(vehicle.VehicleId));
                await _hybridCache.RemoveByPrefixAsync(CachePrefixes.VehiclesList);
            }
            catch(DbUpdateConcurrencyException)
            {
                _logger.Log(CommonErrors.ConcurrencyConflict(vehicle.VehicleId.ToString()), Warning);
                await transaction.RollbackAsync();
                return Result.Failure(CommonErrors.ConcurrencyConflict(vehicle.VehicleId.ToString()));
            }
            catch(Exception e)
            {
                _logger.Error(e, "Removing a vehicle failed. VehicleId: {VehicleId}", vehicle.VehicleId); ;
                await transaction.RollbackAsync();
                return Result.Failure(CommonErrors.RemovingDataFailed(vehicle.VehicleId.ToString()));
            }
        }

        return Result.Success();
    }
}