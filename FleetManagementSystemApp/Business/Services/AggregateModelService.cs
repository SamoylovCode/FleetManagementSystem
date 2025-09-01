using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Business.SubModelHandlers;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Infrastructure.Caching;
using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Business.Services;

public class AggregateModelService<TAggregatorViewModel> : IAggregateModelService<TAggregatorViewModel>
    where TAggregatorViewModel: class, IAggregateViewModel, new()
{
    private readonly ILogger _logger;
    private readonly IEnumerable<ISubModelHandler> _subModelHandlers;
    private readonly IHybridCache _hybridCache;

    public AggregateModelService(
        ILogger logger,
        IEnumerable<ISubModelHandler> subModelHandlers,
        IHybridCache hybridCache)
    {
        _logger = logger;
        _subModelHandlers = subModelHandlers;
        _hybridCache = hybridCache;
    }

    public async Task<Result<TAggregatorViewModel>> BuildAggregateViewModelAsync(TAggregatorViewModel aggregateVm)
    {
        var vehicleId = aggregateVm.GetEntityId();

        if (aggregateVm is null)
        {
            _logger.Error("Aggregate view model is null.");
            return Result<TAggregatorViewModel>.Failure(CommonErrors.ParamIsNullOrEmpty(typeof(AggregateModelService<TAggregatorViewModel>)));
        }

        var cachedAggregateVm = await _hybridCache.GetOrAddAsync(async () =>
        {
            foreach (var handler in _subModelHandlers)
            {
                var subVm = await handler.LoadAsync(aggregateVm);
                if (subVm.IsFailure)
                {
                    _logger.Log(AggregateModelServiceErrors.SubModelIsNull(handler.Prefix, nameof(aggregateVm)), Levels.Warning);
                    continue;
                }

                var prefix = subVm.Value.Prefix;

                if (!string.IsNullOrEmpty(prefix))
                {
                    var specifyProperty = aggregateVm.GetType().GetProperty(prefix);

                    if (specifyProperty is null)
                    {
                        _logger.Warning("Property {Prefix} not found in aggregate view model", prefix);
                        continue;
                    }

                    if (!specifyProperty.PropertyType.IsAssignableFrom(subVm.Value.GetType()))
                    {
                        _logger.Warning("Type mismatch for {Prefix}", prefix);
                        continue;
                    }

                    specifyProperty.SetValue(aggregateVm, subVm.Value);
                }
            }

            return aggregateVm;
        },
        key: vehicleId.ToString(),
        ttl: TimeSpan.FromMinutes(2),
        prefix: CachePrefixes.VehicleAggregateFull(vehicleId));

        if (_subModelHandlers.All(h =>
        {
            var prop = cachedAggregateVm.GetType().GetProperty(h.Prefix);
            return prop is null || prop.GetValue(cachedAggregateVm) is null;
        }))
        {
            _logger.Log(AggregateModelServiceErrors.AggregateSubModelsNotFound(nameof(aggregateVm)), Levels.Error);
            return Result<TAggregatorViewModel>.Failure(AggregateModelServiceErrors.AggregateSubModelsNotFound(nameof(aggregateVm)));
        }

        return Result<TAggregatorViewModel>.Success(cachedAggregateVm);
    }

    public async Task<Result> UpdateAggregateViewModelAsync(TAggregatorViewModel aggregateVm)
    {
        if (aggregateVm is null)
        {
            _logger.Error("Aggregate view model is null.");
            return Result.Failure(CommonErrors.ParamIsNullOrEmpty(typeof(AggregateModelService<TAggregatorViewModel>)));
        }

        var properties = aggregateVm.GetType().GetProperties();

        foreach(var property in properties)
        {
            if (typeof(ISubModel).IsAssignableFrom(property.PropertyType))
            {
                var subModelValue = property.GetValue(aggregateVm) as ISubModel;

                if (subModelValue is null)
                {
                    _logger.Error("Sub model {Prefix} not found.", subModelValue?.Prefix);
                    return Result.Failure(AggregateModelServiceErrors.SubModelIsNull(property.PropertyType.Name, aggregateVm.GetType().Name));
                }

                var updateResult = await UpdateSubModelAsync(subModelValue);

                if (updateResult.IsFailure)
                {
                    _logger.Error("Updating submodel {Prefix} has failed. Errors: {@Errors}", subModelValue.Prefix, updateResult.Errors);
                    return Result.Failure(updateResult.Errors);
                }
            }
        }

        return Result.Success();
    }

    public async Task<Result> UpdateSubModelAsync(ISubModel viewModel)
    {
        var handler = _subModelHandlers.FirstOrDefault(h => h.Prefix == viewModel.Prefix);

        if (handler is null)
        {
            _logger.Error("Sub model {SubModelName} not found.", viewModel.Prefix);

            return Result.Failure(
                AggregateModelServiceErrors.SubModelIsNull(
                    viewModel.GetType().GetProperty(viewModel.Prefix).PropertyType.Name,
                    viewModel.Prefix));
        }

        var savingResult = await handler.SaveAsync(viewModel);

        if (savingResult.IsFailure)
        {
            return Result.Failure(CommonErrors.SavingDataFailed(handler.ViewModelType.Name));
        }

        if (viewModel.VehicleId != Guid.Empty)
        {
            await _hybridCache.RemoveByPrefixAsync(CachePrefixes.VehicleAggregateFull(viewModel.VehicleId));
            await _hybridCache.RemoveByPrefixAsync(CachePrefixes.VehicleAggregateSubModel(viewModel.VehicleId, viewModel.Prefix));
            await _hybridCache.RemoveByPrefixAsync(CachePrefixes.VehiclesList);
        }

        return Result.Success();
    }
}