using Serilog.Events;
using System.ComponentModel.DataAnnotations;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Common.Extensions;

public static class LoggerExtensions
{
    public static void Log(
        this ILogger logger,
        Error error,
        LogEventLevel level = LogEventLevel.Warning)
    {
        logger.Write(
            level,
            error.StructuredLogContext is not null
                ? "Business Erorr {ErrorCode}: {Description} {@Context}"
                : "Business Erorr {ErrorCode}: {Description}",
            error.Code,
            error.DevDescription,
            error.StructuredLogContext);
    }
}

public static class Levels
{
    public const LogEventLevel Verbose = LogEventLevel.Verbose;
    public const LogEventLevel Debug = LogEventLevel.Debug;
    public const LogEventLevel Info = LogEventLevel.Information;
    public const LogEventLevel Warning = LogEventLevel.Warning;
    public const LogEventLevel Error = LogEventLevel.Error;
    public const LogEventLevel Fatal = LogEventLevel.Fatal;
}