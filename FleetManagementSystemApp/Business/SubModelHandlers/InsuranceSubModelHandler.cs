using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services;
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

public class InsuranceSubModelHandler : ISubModelHandler
{
    public string Prefix => "Insurance";
    public Type ViewModelType => typeof(InsuranceViewModel);

    private readonly IVehicleDataAggregator _dataAggregator;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHybridCache _hybridCache;
    private readonly DateRangeParser _dateRangeParser;

    public InsuranceSubModelHandler(
        IVehicleDataAggregator dataAggregator,
        ILogger logger,
        IServiceProvider serviceProvider,
        ApplicationDbContext dbContext,
        IHybridCache hybridCache,
        DateRangeParser dateRangeParser)
    {
        _dataAggregator = dataAggregator;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _dbContext = dbContext;
        _hybridCache = hybridCache;
        _dateRangeParser = dateRangeParser;
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

        var insuranceDto = vehicleDataDto.Value.Insurance;

        if (insuranceDto is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
            return Result<ISubModel>.Failure(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
        }

        var cachedVm = await _hybridCache.GetOrAddAsync(async () =>
        {
            _logger.Information("Returning insurance DTO for vehicle {VehicleId}", vehicleId);

            var periodString = _dateRangeParser.GetPeriodDates(insuranceDto.IssueDate.ToString(), insuranceDto.ExpDate.ToString());

            var insuranceVm = new InsuranceViewModel
            {
                InsuranceId = insuranceDto.InsuranceId,
                VehicleId = insuranceDto.VehicleId,
                Number = insuranceDto.Number,
                IssuedBy = insuranceDto.IssuedBy,
                PeriodString = periodString,
                RowVersion = insuranceDto.RowVersion,
            };
            return insuranceVm;
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
        var insuranceVm = (InsuranceViewModel)viewModel;
        _ = _dateRangeParser.TryParse(insuranceVm.PeriodString, out var startDate, out var endDate);

        var insuranceDto = new InsuranceDto
        {
            InsuranceId = insuranceVm.InsuranceId,
            VehicleId = insuranceVm.VehicleId,
            Number = insuranceVm.Number,
            IssuedBy = insuranceVm.IssuedBy,
            IssueDate = startDate,
            ExpDate = endDate,
            RowVersion = insuranceVm.RowVersion
        };

        var insurance = _dbContext.Insurances
            .FirstOrDefault(i => i.InsuranceId == insuranceDto.InsuranceId);

        if (insurance is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(insuranceDto.VehicleId.ToString(), Prefix));
            return Result.Failure(AggregateModelServiceErrors.SubModelIsNull(insuranceDto.VehicleId.ToString(), Prefix));
        }

        var insuranceMapper = _serviceProvider.GetRequiredService<IBaseMapper<Insurance, InsuranceDto>>();
        insuranceMapper.MapFromDto(insurance, insuranceDto); // Updating entity

        try
        {
            _dbContext.Entry(insurance).OriginalValues["RowVersion"] = insuranceDto.RowVersion;
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.Log(CommonErrors.ConcurrencyConflict(insurance.InsuranceId.ToString()), Levels.Warning);
            return Result.Failure(CommonErrors.ConcurrencyConflict(insurance.InsuranceId.ToString()));
        }

        return Result.Success();
    }
}