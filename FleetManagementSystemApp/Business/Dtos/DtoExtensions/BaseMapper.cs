using FleetManagementSystemApp.Common;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

/// <summary>
/// Convertation methods any data to and from DTO model
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TDto">The type of the DTO.</typeparam>
public abstract class BaseMapper<TModel, TDto>
{
    /// <summary>
    /// Converts to DTO.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns></returns>
    public abstract Result<TDto> ToDto(TModel model);

    /// <summary>
    /// Maps from DTO.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="dto">The DTO.</param>
    /// <returns></returns>
    public abstract Result<TModel> MapFromDto(TModel model, TDto dto);

    /// <summary>
    /// Converts list of models to DTO.
    /// </summary>
    /// <param name="models">The models.</param>
    /// <returns></returns>
    public Result<List<TDto>> ToDto(List<TModel> models)
    {
        List<TDto> result = new List<TDto>();
        List<string> errors = new List<string>();

        foreach (var model in models)
        {
            var dtoResult = ToDto(model);
            if (dtoResult.IsSuccess)
            {
                result.Add(dtoResult.Value);
            }
            else
            {
                errors.Add(dtoResult.Error);
            }
        }

        return errors.Count == 0
            ? Result<List<TDto>>.Success(result)
            : Result<List<TDto>>.Failure($"Ошибки при преобразовании данных в коллекцию DTO: {string.Join("; ", errors)}");
    }

    /// <summary>
    /// Maps list of models from DTO.
    /// </summary>
    /// <param name="models">The models.</param>
    /// <param name="Dtos">The DTOs.</param>
    /// <returns></returns>
    public Result<List<TModel>> MapFromDto(List<TModel> models,
        List<TDto> Dtos)
    {
        if (models.Count != Dtos.Count)
        {
            return Result<List<TModel>>.Failure("Размеры коллекций List<TModel> и List<TDto> не совпадают");
        }

        List<TModel> result = new List<TModel>();
        List<string> errors = new List<string>();

        for (int i = 0; i < models.Count; i++)
        {
            var mapResult = MapFromDto(models[i], Dtos[i]);
            if (mapResult.IsSuccess)
            {
                result.Add(mapResult.Value);
            }
            else
            {
                errors.Add($"Index {i}: {mapResult.Error}");
            }
        }

        return errors.Count == 0
            ? Result<List<TModel>>.Success(result)
            : Result<List<TModel>>.Failure($"Ошибки при преобразовании коллекции данных из DTO: {string.Join("; ", errors)}");
    }
}