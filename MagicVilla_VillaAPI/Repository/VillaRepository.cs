using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {

        private readonly ApplicationDBContext _dbContext;

        public VillaRepository(ApplicationDBContext dbContext) : base(dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Villa> UpdateAsync(Villa villa)
        {
            villa.UpdatedDate = DateTime.Now;
            _dbContext.Villas.Update(villa);
            await _dbContext.SaveChangesAsync();
            return villa;
        }
    }
}
