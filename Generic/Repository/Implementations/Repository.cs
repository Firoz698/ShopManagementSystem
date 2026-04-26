using Microsoft.EntityFrameworkCore;
using ShopManagementSystem.Data;
using ShopManagementSystem.Generic.Repository.Interfaces;

namespace ShopManagementSystem.Generic.Repository.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        protected readonly DbSet<T> _set;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public async Task<List<T>> GetAllAsync() => await _set.ToListAsync();
        public async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);
        public async Task AddAsync(T entity) { await _set.AddAsync(entity); }
        public Task UpdateAsync(T entity) { _set.Update(entity); return Task.CompletedTask; }
        public Task DeleteAsync(T entity) { _set.Remove(entity); return Task.CompletedTask; }
        public async Task SaveAsync() => await _db.SaveChangesAsync();
    }
}
