using FleetManagementSystemApp.Data;
using FleetManagementSystemApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ILogger = Serilog.ILogger;

namespace FleetManagementSystemApp.Validators
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public CurrentUserService(
            IHttpContextAccessor contextAccessor,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            ILogger logger)
        {
            _contextAccessor = contextAccessor;
            _userManager = userManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        public string UserName
        {
            get
            {
                var fullName = _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
                if (string.IsNullOrEmpty(fullName))
                {
                    return string.Empty;
                }

                string[] parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    return string.Empty;
                }

                string lastName = parts[0];
                string initials = string.Join("", parts.Skip(1).Select(p => p.Length > 0 ? $" {p[0]}." : ""));

                return $"{lastName}{initials}";
            }
        }

        public string UserId
        {
            get
            {
                var userId = _contextAccessor.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.Error("Claims has no current user ID.");
                    throw new Exception("Claims do not contain ID for current user.");
                }

                return userId;
            }
        }

        public string CompanyId
        {
            get
            {
                var companyId = _contextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value;

                if (string.IsNullOrEmpty(companyId))
                {
                    _logger.Warning("Claims do not contain a company ID for the current user {UserId}. Attempting to retrieve from the database.", UserId);
                    return _dbContext.Users
                        .Where(u => u.Id == UserId)
                        .Select(u => u.CompanyId.ToString())
                        .FirstOrDefault()!;
                }

                return companyId;
            }
        }


        public Guid CompanyGuid
        {
            get
            {
                return Guid.Parse(CompanyId);
            }
        }

        public string UserRole
        {
            get
            {
                return _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            }
        }

        public string CompanyName
        {
            get
            {
                var companyName = _contextAccessor.HttpContext?.User?.FindFirst("CompanyName")?.Value;
                if (string.IsNullOrEmpty(companyName))
                {
                    _logger.Warning("Claims do not contain a company name for the current user {UserId}. Attempting to retrieve from the database.", UserId);
                    return _userManager.Users
                        .Include(u => u.Company)
                        .FirstOrDefault(u => u.Id == UserId && u.CompanyId.ToString() == CompanyId)?.Company?.Name ?? string.Empty;
                }
                
                return companyName;
            }
        }
    }
}