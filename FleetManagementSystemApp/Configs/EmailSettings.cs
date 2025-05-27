using System.ComponentModel.DataAnnotations;

namespace FleetManagementSystemApp.Configs;

public class EmailSettings
{
    [Required]
    public string SmtpServer { get; set; } = string.Empty;

    [Range(1, 2526)]
    public int Port { get; set; } = default;

    [Required]
    public string SenderEmail { get; set; } = string.Empty;
}