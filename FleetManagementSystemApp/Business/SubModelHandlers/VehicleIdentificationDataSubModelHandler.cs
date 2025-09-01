using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Infrastructure.Caching;
using FleetManagementSystemApp.ViewModels.Vehicle;
using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Business.SubModelHandlers;

public class VehicleIdentificationDataSubModelHandler : ISubModelHandler
{
    public string Prefix => "VehicleIdentificationData";
    public Type ViewModelType => typeof(VehicleIdentificationDataViewModel);

    private readonly IVehicleDataAggregator _dataAggregator;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHybridCache _hybridCache;

    public VehicleIdentificationDataSubModelHandler(
        IVehicleDataAggregator dataAggregator,
        ILogger logger,
        IServiceProvider serviceProvider,
        ApplicationDbContext dbContext,
        IHybridCache hybridCache)
    {
        _dataAggregator = dataAggregator;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _hybridCache = hybridCache;
    }

    public async Task<Result<ISubModel>> LoadAsync(IAggregateViewModel viewModel)
    {
        var vehicleId = viewModel.GetEntityId();
        var vehicleDataDto = await _dataAggregator.GetAsync(vehicleId);

        if (vehicleDataDto.IsFailure)
        {
            _logger.Log(AggregateModelServiceErrors.AggregateSubModelsNotFound(vehicleId.ToString()));
            return Result<ISubModel>.Failure(AggregateModelServiceErrors.AggregateSubModelsNotFound(vehicleId.ToString()));
        }

        var vehicleDto = vehicleDataDto.Value.Vehicle;

        if (vehicleDto is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
            return Result<ISubModel>.Failure(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
        }

        var cachedVm = await _hybridCache.GetOrAddAsync(async () =>
        {
            _logger.Information("Returning vehicle identification data DTO for vehicle {VehicleId}", vehicleId);

            var vehicleIdentificationDataVm = new VehicleIdentificationDataViewModel
            {
                VehicleId = vehicleDto.VehicleId,
                LicencePlate = vehicleDto.LicensePlate,
                Vin = vehicleDto.Vin,
                YearMade = vehicleDto.YearMade,
                RowVersion = vehicleDto.RowVersion,
            };
            return vehicleIdentificationDataVm;
        },
        //key: CachePrefixes.VehicleAggregateSubModelKey(vehicleId, Prefix),
        key: vehicleId.ToString(),
        ttl: TimeSpan.FromMinutes(2),
        prefix: CachePrefixes.VehicleAggregateSubModel(vehicleId, Prefix));

        if (cachedVm == null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
        }

        return Result<ISubModel>.Success(cachedVm);
    }

    public async Task<Result> SaveAsync(ISubModel viewModel)
    {
        var vehicleIdentificationDataVm = (VehicleIdentificationDataViewModel)viewModel;

        var vehicleIdentificationDataDto = new VehicleIdentificationDataDto
        {
            VehicleId = vehicleIdentificationDataVm.VehicleId,
            LicencePlate = vehicleIdentificationDataVm.LicencePlate,
            Vin = vehicleIdentificationDataVm.Vin,
            YearMade = vehicleIdentificationDataVm.YearMade,
            RowVersion = vehicleIdentificationDataVm.RowVersion
        };

        var vehicle = _dbContext.Vehicles
            .FirstOrDefault(v => v.VehicleId == vehicleIdentificationDataDto.VehicleId);

        if (vehicle is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(vehicleIdentificationDataDto.VehicleId.ToString(), Prefix));
            return Result.Failure(AggregateModelServiceErrors.SubModelIsNull(vehicleIdentificationDataDto.VehicleId.ToString(), Prefix));
        }

        var VehicleIdentificationDataMapper = _serviceProvider.GetRequiredService<VehicleIdentificationDataDtoExtentions>();
        VehicleIdentificationDataMapper.MapFromDto(vehicle, vehicleIdentificationDataDto); // Updating entity

        try
        {
            _dbContext.Entry(vehicle).OriginalValues["RowVersion"] = vehicleIdentificationDataDto.RowVersion;
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.Log(CommonErrors.ConcurrencyConflict(vehicle.VehicleId.ToString()), Levels.Warning);
            return Result.Failure(CommonErrors.ConcurrencyConflict(vehicle.VehicleId.ToString()));
        }

        return Result.Success();
    }
}