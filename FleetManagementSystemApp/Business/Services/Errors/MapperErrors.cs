using FleetManagementSystemApp.Common;

namespace FleetManagementSystemApp.Business.Services.Errors;

public static class MapperErrors
{
    public static Error ModelsSizeMismatch() =>
    new Error(
        MapperErrorCodes.ModelsSizeMismatch,
        userDesc: string.Empty,
        devDesc: "The sizes of the model collection and the DTO model do not match.",
        context: null);

    public static Error ModelIsNull() =>
    new Error(
        MapperErrorCodes.ModelIsNull,
        userDesc: string.Empty,
        devDesc: "Mapping model can not be null.",
        context: null);

    public static Error ConversionFailed(Exception ex) =>
        new Error(
        MapperErrorCodes.ConversionFailed,
        userDesc: "Ошибка преобразования данных.",
        devDesc: $"{ex.Message}\nStackTrace: {ex.StackTrace}",
        context: null);
}