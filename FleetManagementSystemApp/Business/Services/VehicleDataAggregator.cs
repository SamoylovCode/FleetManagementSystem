using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using static FleetManagementSystemApp.Common.Extensions.Levels;

using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Business.Services;

public class VehicleDataAggregator : IVehicleDataAggregator
{
    private readonly IHybridCache _hybridCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger _logger;
    private readonly IVehicleService _vehicleService;

    public VehicleDataAggregator(
        IHybridCache hybridCache,
        IServiceProvider serviceProvider,
        ApplicationDbContext dbContext,
        ILogger logger,
        IVehicleService vehicleService)
    {
        _hybridCache = hybridCache;
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _logger = logger;
        _vehicleService = vehicleService;
    }

    public async Task<Result<VehicleDataDto>> GetAsync(Guid vehicleId)
    {
        var cachedData = await _hybridCache.GetOrAddAsync(async () =>
        {
            _logger.Information("Getting vehicle {VehicleId} sub models.", vehicleId);
            var vehicle = await _vehicleService.VehicleQueryWithAll()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);
            
            if (vehicle is null)
            {
                _logger.Log(AggregateModelServiceErrors.AggregateSubModelsNotFound(vehicleId.ToString()), Warning);
                return null;
            }

            var vehicleMapper = _serviceProvider.GetRequiredService<IBaseMapper<Vehicle, VehicleDto>>();
            var insuranceMapper = _serviceProvider.GetRequiredService<IBaseMapper<Insurance, InsuranceDto>>();
            var passportMapper = _serviceProvider.GetRequiredService<IBaseMapper<Passport, PassportDto>>();
            var regCertificateMapper = _serviceProvider.GetRequiredService<IBaseMapper<RegistrationCertificate, RegistrationCertificateDto>>();

            _logger.Information("Mapping to DTO models.");
            var data = new VehicleDataDto
            {
                Vehicle = vehicleMapper.ToDto(vehicle).Value,
                Insurance = insuranceMapper.ToDto(vehicle.Insurance).Value,
                Passport = passportMapper.ToDto(vehicle.Passport).Value,
                Registration = regCertificateMapper.ToDto(vehicle.RegCertificate).Value
            };

            return data;
        },
        key: vehicleId.ToString(),
        ttl: TimeSpan.FromMinutes(2),
        prefix: CachePrefixes.VehicleAggregateFull(vehicleId));

        if (cachedData is null)
        {
            _logger.Log(MapperErrors.MappingFailed(vehicleId.ToString()), Levels.Info);
            return Result<VehicleDataDto>.Failure(MapperErrors.MappingFailed(vehicleId.ToString()));
        }

        return Result<VehicleDataDto>.Success(cachedData);
    }
}