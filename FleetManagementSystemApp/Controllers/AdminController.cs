using FleetManagementSystemApp.Business.Services.Abstract;
using FleetManagementSystemApp.Common.Extensions;
using FleetManagementSystemApp.Data.Entities;
using FleetManagementSystemApp.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FleetManagementSystemApp.Controllers;

[Authorize(Roles = "admin")]
[Route("admin")]
public class AdminController : Controller
{
    private IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> Employees()
    {
        var employees = await _userService.GetAllUsersListAsync();
        return View(employees.Value);
    }

    [HttpGet("company-info")]
    public async Task<IActionResult> CompanyInfo()
    {
        //var company = await _companyService.GetCompanyInfoAsync();
        return View(); // модель — CompanyDto или аналог
    }

    [HttpGet("adduserpartial")]
    public IActionResult AddUserPartial()
    {
        ViewBag.RoleList = new SelectList(
            items: ApplicationRole.AllRoles.Select(x =>
            new
            {
                Value = x.Key,
                Text = x.Value
            }),
            dataValueField: "Value",
            dataTextField: "Text"
        );

        return PartialView("Partials/_AddUserPartial", new AddUserViewModel());
    }

    [HttpPost("adduserpartial")]
    public async Task<IActionResult> AddUserPartial(AddUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RoleList = new SelectList(
                items: ApplicationRole.AllRoles.Select(x =>
                    new
                    {
                        Value = x.Key,
                        Text = x.Value
                    }),
                dataValueField: "Value",
                dataTextField: "Text"
            );

            return PartialView("Partials/_AddUserPartial", model);
        }

        var addUserResult = await _userService.AddUserAsync(model, Request.Scheme);
        return addUserResult.ToActionResult(
            onSuccess: () =>
            {
                TempData["ShowToast"] = "Сотрудник успешно добавлен";
                return Json(new { success = true });
            },
            onFailure: (errors) =>
            {
                foreach (var e in errors)
                {
                    ModelState.AddModelError(e.Code ?? "", e.UserDescription);
                }

                return PartialView("_AddUserPartial", model);
            });
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