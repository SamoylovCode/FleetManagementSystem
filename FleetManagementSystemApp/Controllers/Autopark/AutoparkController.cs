using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.ViewModels;
using FleetManagementSystemApp.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FleetManagementSystemApp.Controllers.Autopark;

[Authorize]
public class AutoparkController : Controller
{
    private IUserService _userService;

    public AutoparkController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Vehicles()
    {
        return View();
    }

    //[HttpPost]
    //public IActionResult Index(Vehicle carId)
    //{
    //    return Ok();
    //}
}