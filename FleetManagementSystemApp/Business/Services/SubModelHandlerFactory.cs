using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.SubModelHandlers;

namespace FleetManagementSystemApp.Business.Services;

public class SubModelHandlerFactory : ISubModelHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public SubModelHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ISubModelHandler GetHandler<ISubModel>()
    {
        var handler = _serviceProvider.GetService<ISubModelHandler>();
        if (handler is null)
        {
            throw new InvalidOperationException($"Обработчик для {typeof(ISubModel).Name} не зарегистрирован.");
        }
        return handler;
    }
}