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

public class CertificateTechInspectionSubModelHandler : ISubModelHandler
{
    public string Prefix => "CertificateTechInspection";
    public Type ViewModelType => typeof(CertificateTechInspection);

    private readonly IVehicleDataAggregator _dataAggregator;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHybridCache _hybridCache;
    private readonly DateRangeParser _dateRangeParser;

    public CertificateTechInspectionSubModelHandler(
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

        var сertificateTechInspectionDto = vehicleDataDto.Value.CertificateTechInspection;

        if (сertificateTechInspectionDto is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
            return Result<ISubModel>.Failure(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
        }

        var cachedVm = await _hybridCache.GetOrAddAsync(async () =>
        {
            _logger.Information("Returning certificate of periodic technical inspection DTO for vehicle {VehicleId}", vehicleId);

            var periodString = _dateRangeParser.GetPeriodDates(сertificateTechInspectionDto.CertificateTechInspectionIssueDate.ToString() ?? string.Empty, сertificateTechInspectionDto.CertificateTechInspectionExpDate.ToString() ?? string.Empty);

            var certificateTechInspectionVm = new CertificateTechInspectionViewModel
            {
                CertificateTechInspectionId = сertificateTechInspectionDto.CertificateTechInspectionId,
                VehicleId = сertificateTechInspectionDto.VehicleId,
                CertificateTechInspectionNum = сertificateTechInspectionDto.CertificateTechInspectionNum,
                CertificateTechInspectionIssuedBy = сertificateTechInspectionDto.CertificateTechInspectionIssuedBy,
                CertificateTechInspectionIssueDate = сertificateTechInspectionDto.CertificateTechInspectionIssueDate,
                CertificateTechInspectionExpDate = сertificateTechInspectionDto.CertificateTechInspectionExpDate,
                RowVersion = сertificateTechInspectionDto.RowVersion,
                PeriodString = periodString,
            };
            return certificateTechInspectionVm;
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
        var CertificateTechInspectionVm = (CertificateTechInspectionViewModel)viewModel;
        _ = _dateRangeParser.TryParse(CertificateTechInspectionVm.PeriodString, out var startDate, out var endDate);

        var certificateTechInspectionDto = new CertificateTechInspectionDto
        {
            CertificateTechInspectionId = CertificateTechInspectionVm.CertificateTechInspectionId,
            VehicleId = CertificateTechInspectionVm.VehicleId,
            CertificateTechInspectionNum = CertificateTechInspectionVm.CertificateTechInspectionNum,
            CertificateTechInspectionIssuedBy = CertificateTechInspectionVm.CertificateTechInspectionIssuedBy,
            CertificateTechInspectionIssueDate = startDate,
            CertificateTechInspectionExpDate = endDate,
            RowVersion = CertificateTechInspectionVm.RowVersion
        };

        var certificateTechInspection = _dbContext.CertificateTechInspections
            .FirstOrDefault(c => c.CertificateTechInspectionId == certificateTechInspectionDto.CertificateTechInspectionId);

        if (certificateTechInspection is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(certificateTechInspectionDto.VehicleId.ToString(), Prefix));
            return Result.Failure(AggregateModelServiceErrors.SubModelIsNull(certificateTechInspectionDto.VehicleId.ToString(), Prefix));
        }

        var certificateTechInspectionMapper = _serviceProvider.GetRequiredService<IBaseMapper<CertificateTechInspection, CertificateTechInspectionDto>>();
        certificateTechInspectionMapper.MapFromDto(certificateTechInspection, certificateTechInspectionDto); // Updating entity

        try
        {
            _dbContext.Entry(certificateTechInspection).OriginalValues["RowVersion"] = certificateTechInspectionDto.RowVersion;
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.Log(CommonErrors.ConcurrencyConflict(certificateTechInspection.CertificateTechInspectionId.ToString()), Levels.Warning);
            return Result.Failure(CommonErrors.ConcurrencyConflict(certificateTechInspection.CertificateTechInspectionId.ToString()));
        }

        return Result.Success();
    }
}