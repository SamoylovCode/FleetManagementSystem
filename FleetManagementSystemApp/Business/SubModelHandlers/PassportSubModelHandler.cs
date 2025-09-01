using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Infrastructure.Caching;
using FleetManagementSystemApp.ViewModels.Vehicle;
using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Business.SubModelHandlers;

public class PassportSubModelHandler : ISubModelHandler
{
    public string Prefix => "Passport";
    public Type ViewModelType => typeof(PassportViewModel);

    private readonly IVehicleDataAggregator _dataAggregator;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHybridCache _hybridCache;

    public PassportSubModelHandler(
        IVehicleDataAggregator dataAggregator,
        IServiceProvider serviceProvider,
        ApplicationDbContext dbContext,
        IHybridCache hybridCache,
        ILogger logger)
    {
        _dataAggregator = dataAggregator;
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _hybridCache = hybridCache;
        _logger = logger;
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

        var passportDto = vehicleDataDto.Value.Passport;

        if (passportDto is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
            return Result<ISubModel>.Failure(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
        }

        var cachedVm = await _hybridCache.GetOrAddAsync(async () =>
        {
            _logger.Information("Returning passport DTO for vehicle {VehicleId}", vehicleId);

            var passportVm = new PassportViewModel
            {
                PassportId = passportDto.PassportId,
                VehicleId = passportDto.VehicleId,
                Number = passportDto.Number,
                IssueDate = passportDto.IssueDate,
                RowVersion = passportDto.RowVersion
            };
            return passportVm;
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
        var passportVm = (PassportViewModel)viewModel;

        var passportDto = new PassportDto
        {
            PassportId = passportVm.PassportId,
            VehicleId = passportVm.VehicleId,
            Number = passportVm.Number,
            IssueDate = passportVm.IssueDate,
            RowVersion = passportVm.RowVersion
        };
        
        var passport = _dbContext.Passports
            .FirstOrDefault(p => p.PassportId == passportDto.PassportId);

        if (passport is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(passportDto.VehicleId.ToString(), Prefix));
            return Result.Failure(AggregateModelServiceErrors.SubModelIsNull(passportDto.VehicleId.ToString(), Prefix));
        }

        var passportMapper = _serviceProvider.GetRequiredService<IBaseMapper<Passport, PassportDto>>();
        passportMapper.MapFromDto(passport, passportDto); // Updating entity

        try
        {
            _dbContext.Entry(passport).OriginalValues["RowVersion"] = passportDto.RowVersion;
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.Log(CommonErrors.ConcurrencyConflict(passport.PassportId.ToString()), Levels.Warning);
            return Result.Failure(CommonErrors.ConcurrencyConflict(passport.PassportId.ToString()));
        }

        return Result.Success();
    }
}