namespace FleetManagementSystemApp.Data.Repositories.Abstract;

/// <summary>
/// Generic interface for CRUD operations
/// </summary>
/// <typeparam name="T">Type of methods</typeparam>
/// <typeparam name="TKey">Type of parameters</typeparam>
public interface IRepository<T, TKey> : IDisposable where T : class
{
    public Task<IEnumerable<T>> GetAllListAsync();
    public Task<T> GetByIdAsync(TKey id);
    public Task CreateAsync(T item);
    public Task UpdateAsync(T item);
    public Task DeleteAsync(T item);
    public Task Save();
}