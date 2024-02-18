using MagicVilla_VillaAPI.Models;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        //Expression is used to enable linq commands
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, int pageSize = 0, int pageNumber = 1);

        //Tracked is used to define whether or not an entity should be tracked using AsNoTracking()
        Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null);

        Task CreateAsync(T villa);
        Task RemoveAsync(T villa);
        Task SaveAsync();
    }
}
