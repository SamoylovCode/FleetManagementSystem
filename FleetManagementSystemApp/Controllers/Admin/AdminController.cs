using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.ViewModels;
using FleetManagementSystemApp.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FleetManagementSystemApp.Controllers.Admin;

[Authorize(Roles = "admin")]
public class AdminController : Controller
{
    private IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult AddUserToCompany()
    {
        ViewBag.RoleList = new SelectList(
            items: ApplicationRole.AllRoles.Select(x => new { Value = x.Key, Text = x.Value }),
            dataValueField: "Value",
            dataTextField: "Text"
        );

        return PartialView("_AddUserPartial", new AddUserViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> AddUserToCompany(AddUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RoleList = new SelectList(
                items: ApplicationRole.AllRoles.Select(x => new { Value = x.Key, Text = x.Value }),
                dataValueField: "Value",
                dataTextField: "Text"
            );

            return View("Index", new PartialPageModel
            {
                PartialViewName = "_AddUserPartial",
                ViewModel = model
            });
        }

        var addUserResult = await _userService.AddUserAsync(model, Request.Scheme);
        if (!addUserResult.Succeeded)
        {
            return BadRequest(addUserResult.Errors);
        }

        TempData["ShowToast"] = "Сотрудник успешно добавлен";
        return RedirectToAction("AddUserToCompany");
    }

    //[HttpPatch]
    //public async Task<IActionResult> UpdateUserData(UpdateUserDataViewModel model)
    //{
    //    var addUserResult = await _userService.UpdateUserDataAsync(model);
    //    if (!addUserResult.Succeeded)
    //    {
    //        return BadRequest(addUserResult);
    //    }

    //    return Ok();
    //}
}
