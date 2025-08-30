using FleetManagementSystemApp.Business.Services.Errors;
using FleetManagementSystemApp.Common.Extensions;
using System.Globalization;
using static FleetManagementSystemApp.Common.Extensions.Levels;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Business.Services;

public class DateRangeParser
{
    private readonly ILogger _logger;

    public DateRangeParser(ILogger logger)
    {
        _logger = logger;
    }

    public bool TryParse(string? periodString, out DateOnly? start, out DateOnly? end)
    {
        start = default;
        end = default;
        var format = "dd.MM.yyyy";

        if (string.IsNullOrEmpty(periodString))
        {
            _logger.Log(CommonErrors.ParamIsNullOrEmpty(typeof(DateRangeParser)), Warning);
            return false;
        }

        var parts = periodString.Split(new[] { " - " }, StringSplitOptions.None);
        if (parts.Length == 2)
        {
            if (DateOnly.TryParseExact(parts[0].Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly startDate) && DateOnly.TryParseExact(parts[1].Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly endDate))
            {
                start = startDate;
                end = endDate;
                _logger.Information("Parsed period string '{s}' -> {start} - {end}", periodString, start, end);
                return true;
            }
        }

        _logger.Information("Parsing period string failed.");
        start = null;
        end = null;
        return false;
    }

    public string GetPeriodDates(string startDate, string endDate)
    {
        if (string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
        {
            _logger.Log(CommonErrors.ParamIsNullOrEmpty(typeof(DateRangeParser)), Warning);
            return string.Empty;
        }

        return $"{startDate} - {endDate}";
    }
}