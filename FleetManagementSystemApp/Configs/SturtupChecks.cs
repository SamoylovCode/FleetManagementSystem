namespace FleetManagementSystemApp.Configs;

public static class StartupChecks
{
    public static void ValidateRequiredSettings(IConfiguration config)
    {
        var requiredVars = new[] { "MAILTRAP_USERNAME", "MAILTRAP_PASSWORD" };

        foreach (var variable in requiredVars)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variable)))
                throw new InvalidOperationException($"Необходимая переменная окружения '{variable}' не задана.");
        }
    }
}