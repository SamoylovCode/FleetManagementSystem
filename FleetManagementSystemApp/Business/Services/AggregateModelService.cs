using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Business.SubModelHandlers;
using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Infrastructure.Caching;
using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Business.Services;

/// <summary>
/// Service responsible for building and updating aggregate view models by combining multiple sub-models.
/// Provides caching functionality to improve performance and ensures data consistency through sub-model handlers.
/// </summary>
/// <typeparam name="TAggregatorViewModel">The type of aggregate view model that implements IAggregateViewModel.</typeparam>
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

    /// <summary>
    /// Builds an aggregate view model by loading sub-models and caching the result.
    /// </summary>
    /// <param name="aggregateVm">The aggregate view model to populate with sub-models.</param>
    /// <returns>A result containing the populated aggregate view model or failure information.</returns>
    public async Task<Result<TAggregatorViewModel>> BuildAggregateViewModelAsync(TAggregatorViewModel aggregateVm)
    {
        var entityId = aggregateVm.GetEntityId();

        if (aggregateVm is null)
        {
            _logger.Error("Aggregate view model is null.");
            return Result<TAggregatorViewModel>.Failure(CommonErrors.ParamIsNullOrEmpty(typeof(AggregateModelService<TAggregatorViewModel>)));
        }

        TAggregatorViewModel cachedAggregateVm = await _hybridCache.GetOrAddAsync(async () =>
        {
            foreach (var handler in _subModelHandlers)
            {
                var subVm = await handler.LoadAsync(aggregateVm);
                if (subVm.IsFailure)
                {
                    _logger.Log(AggregateModelServiceErrors.SubModelIsNull(handler.Prefix, nameof(aggregateVm)), Levels.Warning);
                    _logger.Warning("Error: @{Error}.", subVm.Errors);
                    continue;
                }

                var prefix = subVm.Value.Prefix;

                if (!string.IsNullOrEmpty(prefix))
                {
                    System.Reflection.PropertyInfo? aggregateSubModel = aggregateVm.GetType().GetProperty(prefix);

                    if (aggregateSubModel is null)
                    {
                        _logger.Warning("Property {Prefix} not found in aggregate view model", prefix);
                        continue;
                    }

                    if (!aggregateSubModel.PropertyType.IsAssignableFrom(subVm.Value.GetType()))
                    {
                        _logger.Warning("Type mismatch for {Prefix}", prefix);
                        continue;
                    }

                    // Set the loaded sub-model value into the corresponding property of the aggregate sub view model
                    aggregateSubModel.SetValue(aggregateVm, subVm.Value);
                }
            }

            return aggregateVm;
        },
        key: entityId.ToString(),
        ttl: TimeSpan.FromMinutes(2),
        prefix: CachePrefixes.VehicleAggregateFull(entityId));

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

    /// <summary>
    /// Updates all sub-models within the aggregate view model by calling their respective handlers.
    /// </summary>
    /// <param name="aggregateVm">The aggregate view model containing sub-models to update.</param>
    /// <returns>A result indicating success or failure of the update operation.</returns>
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

    /// <summary>
    /// Updates a single sub-model by finding the appropriate handler and saving the changes.
    /// Also removes related cached entries to ensure data consistency.
    /// </summary>
    /// <param name="viewModel">The sub-model view model to update.</param>
    /// <returns>A result indicating success or failure of the update operation.</returns>
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