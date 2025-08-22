using FleetManagementSystemApp.Common;
using System.Runtime.CompilerServices;

namespace FleetManagementSystemApp.Business.Services.Errors;

public static class CommonErrors
{
    // 'memberName' - атрибут передает имя метода, не нужно указывать данный параметр
    public static Error ParamIsNullOrEmpty(Type entityType = null, [CallerMemberName] string memberName = "") =>
        new Error(
            CommonErrorCodes.ParamIsNullOrEmpty,
            userDesc: "Параметр не передан.",
            devDesc: entityType == null
                        ? $"Parameter is missing in '{memberName}'."
                        : $"Parameter of '{entityType.Name}' type is missing in '{memberName}'.",
            context: new { EntityType = entityType, MemberName = memberName });

    public static Error ConcurrencyConflict(string entityId) =>
        new Error(
            CommonErrorCodes.ConcurrencyConflict,
            userDesc: "Данные были изменены другим пользователем.",
            devDesc: "RowVersion mismatch. The data was modified by another user.",
            context: new { entityId });

    public static Error InvalidType(string entityType) =>
        new Error(
            CommonErrorCodes.InvalidType,
            userDesc: "Ошибка преобразования типов.",
            devDesc: $"Type conversion '{entityType}' failed.",
            context: new { EntityType = entityType });

    public static Error SavingDataFailed(string entityName) =>
        new Error(
            CommonErrorCodes.SavingDataFailed,
            userDesc: "Возникла ошибка при сохранении данных.",
            devDesc: $"An error occurred while saving the entity '{entityName}' data.",
            context: new { EntityName = entityName });

    public static Error RemovingDataFailed(string entityName) =>
        new Error(
            CommonErrorCodes.RemovingDataFailed,
            userDesc: "Возникла ошибка при удалении данных.",
            devDesc: $"An error occurred while deleting the entity '{entityName}' data from database.",
            context: new { EntityName = entityName });
}