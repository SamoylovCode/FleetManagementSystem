using FleetManagementSystemApp.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace FleetManagementSystemApp.Common.Extensions;

public static class TempDataExtensions
{
    public static void SetAlert(
        this ITempDataDictionary tempData,
        string message, bool isSuccess)
    {
        tempData["AlertModel"] = JsonSerializer.Serialize(
            new AlertViewModel
            {
                Message = message,
                IsSuccess = isSuccess
            });
    }
}