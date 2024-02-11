using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        internal DbSet<T> _dbSet;
        private readonly ApplicationDBContext _dbContext;

        public Repository(ApplicationDBContext dbContext)
        {
            this._dbContext = dbContext;
            
            //_dbContext.VillaNumbers.Include(u=>u.Villa).ToList();
            
            //Through the type that dbSet receives when it is created, it is recognized which entity is the one that has to be returned
            _dbSet = _dbContext.Set<T>();

        }

        public async Task CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true, string? includeProperties = null)
        {
            IQueryable<T> entity = _dbSet;

            if (!tracked)
            {
                entity = entity.AsNoTracking();
            }

            if (filter != null)
            {
                entity = entity.Where(filter);
            }

            if(includeProperties != null) {
                foreach (var includeProp in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    entity = entity.Include(includeProp);
                }
            }

            return await entity.FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> entity = _dbSet;

            if (filter != null)
            {
                entity = entity.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    entity = entity.Include(includeProp);
                }
            }

            return await entity.ToListAsync();
        }

        public async Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
