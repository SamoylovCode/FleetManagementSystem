using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common;

namespace FleetManagementSystemApp.Business.Dtos.DtoExtensions;

public interface IBaseMapper<TModel, TDto>
{
    public Result<TDto> ToDto(TModel model);
    public Result<TModel> MapFromDto(TModel model, TDto dto);
    public Result<List<TDto>> ToDto(List<TModel> models);
    public Result<List<TModel>> MapFromDto(List<TModel> models, List<TDto> dtos);
}

/// <summary>
/// Convertation methods any data to and from DTO model
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
/// <typeparam name="TDto">The type of the DTO.</typeparam>
public abstract class BaseMapper<TModel, TDto> : IBaseMapper<TModel, TDto>
{
    /// <summary>
    /// Converts to DTO.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>TDto</returns>
    public abstract Result<TDto> ToDto(TModel model);

    /// <summary>
    /// Maps from DTO.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="dto">The DTO.</param>
    /// <returns>TModel</returns>
    public abstract Result<TModel> MapFromDto(TModel model, TDto dto);

    /// <summary>
    /// Converts list of models to DTO.
    /// </summary>
    /// <param name="models">The models.</param>
    /// <returns>List<typeparamref name="TDto"/></returns>
    public Result<List<TDto>> ToDto(List<TModel> models)
    {
        List<TDto> result = new List<TDto>();
        List<Error> errors = new List<Error>();

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
            : Result<List<TDto>>.Failure(errors);
    }

    /// <summary>
    /// Maps list of models from DTO.
    /// </summary>
    /// <param name="models">The models.</param>
    /// <param name="dtos">The DTOs.</param>
    /// <returns>List<typeparamref name="TModel"/></returns>
    public Result<List<TModel>> MapFromDto(List<TModel> models, List<TDto> dtos)
    {
        if (models.Count != dtos.Count)
        {
            return Result<List<TModel>>.Failure(MapperErrors.ModelsSizeMismatch());
        }

        List<TModel> result = new List<TModel>();
        List<Error> errors = new List<Error>();

        for (int i = 0; i < models.Count; i++)
        {
            var mapResult = MapFromDto(models[i], dtos[i]);
            if (mapResult.IsSuccess)
            {
                result.Add(mapResult.Value);
            }
            else
            {
                errors.Add(mapResult.Error);
            }
        }

        return errors.Count == 0
            ? Result<List<TModel>>.Success(result)
            : Result<List<TModel>>.Failure(errors);
    }
}