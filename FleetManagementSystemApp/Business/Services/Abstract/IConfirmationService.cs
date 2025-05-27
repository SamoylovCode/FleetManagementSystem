namespace FleetManagementSystemApp.Business.Services.Abstract;

public interface IConfirmationService
{
    public Task<bool> ConfirmAsync(string userId, string token);
}
