using FleetManagementSystemApp.Common;

namespace FleetManagementSystemApp.Business.Services.Errors;

public static class AggregateModelServiceErrors
{
    public static Error SubModelIsNull(string subModel, string entity) =>
        new Error(
            AggregateModelServiceErrorCodes.SubModelIsNull,
            userDesc: $"Не найдены данные подмодели '{subModel}'.",
            devDesc: $"Has no data submodel '{subModel}' of entity '{entity}'.",
            context: new { SubModel = subModel, Entity = entity });

    public static Error AggregateSubModelsNotFound(string entity) =>
        new Error(
            AggregateModelServiceErrorCodes.AggregateSubModelsNotFound,
            userDesc: "Не найдены данные об указанной агрегатной модели.",
            devDesc: $"No data found for aggregate model '{entity}'.",
            context: new { Entity = entity });
}