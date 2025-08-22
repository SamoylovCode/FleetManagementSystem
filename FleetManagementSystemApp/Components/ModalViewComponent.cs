using FleetManagementSystemApp.ViewModels.Components;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagementSystemApp.Components;

public class ModalViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string title, string modalId = "universalModal")
    {
        return View(new ModalViewModel
        {
            ModalId = modalId,
            Title = title
        });
    }
}