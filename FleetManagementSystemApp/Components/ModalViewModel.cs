using Microsoft.AspNetCore.Mvc;
using FleetManagementSystemApp.ViewModels.Components;

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