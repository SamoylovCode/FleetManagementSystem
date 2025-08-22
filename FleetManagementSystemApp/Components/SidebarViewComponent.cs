using FleetManagementSystemApp.Validators;
using FleetManagementSystemApp.ViewModels.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.AccessControl;

namespace FleetManagementSystemApp.Components;

public class SidebarViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;

    public SidebarViewComponent(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }
    public IViewComponentResult Invoke()
    {
        return View(new SidebarViewModel
        {
            UserName = _currentUserService.UserName,
            UserRole = _currentUserService.UserRole,
            CompanyId = _currentUserService.CompanyId,
            CompanyName = _currentUserService.CompanyName
        });
    }
}