using FleetManagementSystemApp.Business.Dtos;
using FleetManagementSystemApp.Business.Dtos.DtoExtensions;
using FleetManagementSystemApp.Business.Services;
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

public class RegistrationCertificateSubModelHandler : ISubModelHandler
{
    public string Prefix => "RegistrationCertificate";
    public Type ViewModelType => typeof(RegistrationCertificateViewModel);

    private readonly IVehicleDataAggregator _dataAggregator;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHybridCache _hybridCache;

    public RegistrationCertificateSubModelHandler(
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

        var regDto = vehicleDataDto.Value.Registration;

        if (regDto is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
            return Result<ISubModel>.Failure(AggregateModelServiceErrors.SubModelIsNull(vehicleId.ToString(), Prefix));
        }

        var cachedVm = await _hybridCache.GetOrAddAsync(async () =>
        {
            _logger.Information("Returning registration certificate DTO for vehicle {VehicleId}", vehicleId);

            var regViewModel = new RegistrationCertificateViewModel
            {
                RegCertificateId = regDto.RegCertificateId,
                VehicleId = regDto.VehicleId,
                Number = regDto.Number,
                IssueDate = regDto.IssueDate,
                RowVersion = regDto.RowVersion
            };
            return regViewModel;
        },
        key: CachePrefixes.VehicleAggregateSubModelKey(vehicleId, Prefix),
        ttl: TimeSpan.FromMinutes(2),
        prefix: CachePrefixes.VehicleAggregateSubModel(vehicleId, Prefix));

        return Result<ISubModel>.Success(cachedVm);
    }

    public async Task<Result> SaveAsync(ISubModel viewModel)
    {
        var regVm = (RegistrationCertificateViewModel)viewModel;

        var regDto = new RegistrationCertificateDto
        {
            RegCertificateId = regVm.RegCertificateId,
            VehicleId = regVm.VehicleId,
            Number = regVm.Number,
            IssueDate = regVm.IssueDate,
            RowVersion = regVm.RowVersion
        };

        var reg = _dbContext.RegistrationCertificates
            .FirstOrDefault(r => r.RegCertificateId == regDto.RegCertificateId);

        if (reg is null)
        {
            _logger.Log(AggregateModelServiceErrors.SubModelIsNull(regDto.VehicleId.ToString(), Prefix));
            return Result.Failure(AggregateModelServiceErrors.SubModelIsNull(regDto.VehicleId.ToString(), Prefix));
        }

        var regMapper = _serviceProvider.GetRequiredService<IBaseMapper<RegistrationCertificate, RegistrationCertificateDto>>();
        regMapper.MapFromDto(reg, regDto); // Updating entity

        try
        {
            _dbContext.Entry(reg).OriginalValues["RowVersion"] = regDto.RowVersion;
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.Log(CommonErrors.ConcurrencyConflict(reg.RegCertificateId.ToString()), Levels.Warning);
            return Result.Failure(CommonErrors.ConcurrencyConflict(reg.RegCertificateId.ToString()));
        }

        return Result.Success();
    }
}