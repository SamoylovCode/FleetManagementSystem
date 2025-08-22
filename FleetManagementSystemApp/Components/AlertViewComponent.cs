using FleetManagementSystemApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FleetManagementSystemApp.Components;

public class AlertViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var serializedAlertModel = TempData["AlertModel"] as string;
        if (string.IsNullOrEmpty(serializedAlertModel))
        {
            return Content(string.Empty);
        }

        var model = JsonSerializer.Deserialize<AlertViewModel>(serializedAlertModel);
        return View(model);
    }
}