using FleetManagementSystemApp.Common;
using FleetManagementSystemApp.ViewModels.Vehicle.Abstract;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IAggregateModelService<TAggregateViewModel>
    where TAggregateViewModel : class, IAggregateViewModel
{
    public Task<Result<TAggregateViewModel>> BuildAggregateViewModelAsync(TAggregateViewModel viewModel);
    public Task<Result> UpdateAggregateViewModelAsync(TAggregateViewModel viewModel);
    public Task<Result> UpdateSubModelAsync(ISubModel viewModel);
}