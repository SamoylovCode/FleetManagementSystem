namespace FleetManagementSystemApp.Infrastructure.Caching;

public static class CachePrefixes
{
    // Используется в качестве ключа, <идентификатор агрегатной модели>:<префикс подмодели>,
    // например, {vehicleId}:{prefix}
    public static string VehicleAggregateSubModelKey(Guid vehicleId, string prefix)
    {
        return $"{vehicleId}:{prefix}";
    }

    // Используется в качестве префикса подмодели, <идентификатор агрегатной модели>:<префикс подмодели>,
    // например, vehicle:aggregate:{vehicleId}:{prefix}
    public static string VehicleAggregateSubModel(Guid vehicleId, string prefix)
    {
        return $"vehicle:aggregate:{vehicleId}:{prefix}";
    }

    // Используется в качестве префикса к агрегатной модели, <идентификатор агрегатной модели>
    public static string VehicleAggregateFull(Guid vehicleId)
    {
        return $"vehicle:aggregate:{vehicleId}:full";
    }

    public const string VehiclesList = "vehicles:list";
    public const string UsersList = "users:list";
    public const string UserById = "user:by-id";
    public const string UserByEmail = "user:by-email";
}