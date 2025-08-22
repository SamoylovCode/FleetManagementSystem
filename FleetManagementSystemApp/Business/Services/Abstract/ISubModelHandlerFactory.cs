using FleetManagementSystemApp.Business.SubModelHandlers;

namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface ISubModelHandlerFactory
{
    ISubModelHandler GetHandler<ISubModel>();
}