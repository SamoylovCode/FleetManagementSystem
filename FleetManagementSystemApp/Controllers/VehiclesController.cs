using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.Filters;
using FleetManagementSystemApp.ViewModels.Vehicle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Controllers;

[Authorize]
[Route("vehicles")]
public class VehiclesController : Controller
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger _logger;
    private readonly IAggregateModelService<VehiclePageViewModel> _aggregateModelService;

    public VehiclesController(
        IVehicleService vehicleService,
        ILogger logger,
        IAggregateModelService<VehiclePageViewModel> aggregateModelService)
    {
        _vehicleService = vehicleService;
        _logger = logger;
        _aggregateModelService = aggregateModelService;
    }

    [HttpGet("")]
    public async Task<IActionResult> List()
    {
        var vehicles = await _vehicleService.GetAllVehiclesAsync();
        return View(vehicles.Value);
    }

    [HttpGet("create")]
    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> Create()
    {
        var vehiclePageVm = _vehicleService.GetNewVehiclePage();
        var resultAggregateVm = await _aggregateModelService.BuildAggregateViewModelAsync(vehiclePageVm);

        return await resultAggregateVm.ToActionResultAsync(
            onSuccess: () =>
            {
                return Task.FromResult<IActionResult>(View(resultAggregateVm.Value));
            },
            onFailure: async (errors) =>
            {
                foreach (var e in errors)
                {
                    ModelState.AddModelError("", e.UserDescription ?? e.DevDescription);
                }

                return View(resultAggregateVm);
            });
    }

    [HttpPost("create")]
    [ValidateParamsFilter] // TODO: добавить в остальные методы с параметрами, которые необходимо проверить
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> Create(VehiclePageViewModel viewModel)
    {
        var result = await _vehicleService.CreateVehicleAsync(viewModel);

        return await result.ToActionResultAsync(
            onSuccess: () =>
            {
                TempData.SetAlert("Транспортное средство успешно добавлено.", true);
                return Task.FromResult<IActionResult>(RedirectToAction("Edit", new { vehicleId = viewModel.VehicleId } ));
            },
            onFailure: async (errors) =>
            {
                TempData.SetAlert("Не удалось добавить транспортное средство.", false);
                foreach (var e in errors)
                {
                    ModelState.AddModelError("", e.UserDescription ?? e.DevDescription);
                }

                return View(viewModel);
            });
    }

    [HttpGet("{vehicleId}")]
    public async Task<IActionResult> Details(Guid vehicleId, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var aggregateVm = await _aggregateModelService.BuildAggregateViewModelAsync(new VehiclePageViewModel { VehicleId = vehicleId });

        return View(aggregateVm.Value);
    }

    [HttpGet("{vehicleId}/edit")]
    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> Edit(Guid vehicleId, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        var resultAggregateVm = await _aggregateModelService.BuildAggregateViewModelAsync(new VehiclePageViewModel { VehicleId = vehicleId });

        return await resultAggregateVm.ToActionResultAsync(
            onSuccess: () =>
            {
                return Task.FromResult<IActionResult>(View(resultAggregateVm.Value));
            },
            onFailure: async (errors) =>
            {
                _logger.Error("Error occurred while obtaining the vehicle model for editing. Errors: {@Errors}", resultAggregateVm.Errors);
                foreach (var e in resultAggregateVm.Errors)
                {
                    ModelState.AddModelError("", e.UserDescription ?? "");
                }

                return View(resultAggregateVm);
            });
    }

    // POST /vehicles/{vehicleId}/edit
    // Обновляет ТС, вызывается из формы Edit.cshtml
    [HttpPost("{vehicleId}/edit")]
    [Authorize(Roles = "admin, manager")]
    [ValidateAntiForgeryToken]
    [ActionName("Edit")]
    public async Task<IActionResult> Update(VehiclePageViewModel viewModel, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var updateResult = await _aggregateModelService.UpdateAggregateViewModelAsync(viewModel);

        return await updateResult.ToActionResultAsync(
            onSuccess: () =>
            {
                TempData.SetAlert("Данные сохранены.", true);
                return Task.FromResult<IActionResult>(RedirectToAction("Edit", new { vehicleId = viewModel.VehicleId }));
            },
            onFailure: async (errors) =>
            {
                _logger.Error("Updating submodels has failed. Errors: {@Errors}", updateResult.Errors);
                TempData.SetAlert("Не удалось обновить данные.", false);

                foreach (var e in updateResult.Errors)
                {
                    ModelState.AddModelError("", e.UserDescription ?? "");
                }

                return View(viewModel);
            });
    }

    [HttpGet]
    [Authorize(Roles ="admin, manager")]
    [Route("delete/{vehicleId}")]
    public async Task<IActionResult> Delete(Guid vehicleId)
    {
        var getVehicleResult = await _vehicleService.GetVehicleAsync(vehicleId);

        return await getVehicleResult.ToActionResultAsync(
            onSuccess: () =>
            {
                var vehicle = getVehicleResult.Value;

                var vehicleRemoveVm = new VehicleRemoveViewModel
                {
                    VehicleId = vehicle.VehicleId,
                    RowVersion = vehicle.RowVersion
                };

                return Task.FromResult<IActionResult>(PartialView("Partials/_DeletePartial", vehicleRemoveVm));
            },
            onFailure: async (errors) =>
            {
                foreach (var e in getVehicleResult.Errors)
                {
                    ModelState.AddModelError("", e.UserDescription ?? "");
                }

                return View(getVehicleResult.Value);
            });
    }

    [HttpPost]
    [Authorize(Roles = "admin, manager")]
    [ValidateAntiForgeryToken]
    [Route("delete/{vehicleId}/confirm")]
    public async Task<IActionResult> DeleteConfirmed(VehicleRemoveViewModel viewModel)
    {
        var vehicleStub = new Vehicle
        {
            VehicleId = viewModel.VehicleId,
            RowVersion = viewModel.RowVersion
        };

        var removingResult = await _vehicleService.RemoveVehicleAsync(vehicleStub);

        return await removingResult.ToActionResultAsync(
            onSuccess: async () =>
            {
                _logger.Information($"The vehicle with ID '{vehicleStub.VehicleId}' has been removed.");
                TempData.SetAlert($"ТС удалено!", true);

                return Json(new { success = true });
            },
            onFailure: async (errors) =>
            {
                _logger.Log(VehicleServiceErrors.FailedToRemoveVehicle(vehicleStub.VehicleId.ToString()));
                TempData.SetAlert("Не удалось удалить ТС.", false);

                foreach (var e in removingResult.Errors)
                {
                    ModelState.AddModelError("", e.UserDescription ?? "");
                }

                return RedirectToAction("Details", new { vehicleId = vehicleStub.VehicleId });
            });
    }
}