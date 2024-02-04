using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    public class VillaNumberRepository : Repository<VillaNumber>, IVillaNumberRepository
    {

        private readonly ApplicationDBContext _dbContext;

        public VillaNumberRepository(ApplicationDBContext dbContext) : base(dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<VillaNumber> UpdateAsync(VillaNumber villaNro)
        {
            villaNro.UpdatedDate = DateTime.Now;
            _dbContext.VillaNumbers.Update(villaNro);
            await _dbContext.SaveChangesAsync();
            return villaNro;
        }
    }
}
