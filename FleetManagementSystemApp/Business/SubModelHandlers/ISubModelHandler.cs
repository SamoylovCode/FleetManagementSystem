using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;

namespace FleetManagementSystemApp.Business.SubModelHandlers;

public interface ISubModelHandler
{
    string Prefix { get; }
    Type ViewModelType { get; }

    Task<Result<ISubModel>> LoadAsync(IAggregateViewModel viewModel);
    Task<Result> SaveAsync(ISubModel viewModel);
}