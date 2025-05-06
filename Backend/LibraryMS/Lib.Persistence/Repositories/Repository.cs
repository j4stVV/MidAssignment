using Lib.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lib.Persistence.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _entities;

    public Repository(AppDbContext context)
    {
        _context = context;
        _entities = context.Set<T>();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id) => await _entities.FindAsync(id);
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _entities.ToListAsync();
    public virtual async Task AddAsync(T entity) => await _entities.AddAsync(entity);
    public virtual void Update(T entity) => _entities.Update(entity);
    public virtual async Task DeleteAsync(T entity)
    {
        _entities.Remove(entity);
        await _context.SaveChangesAsync();
    }
    public IQueryable<T> Query() => _entities.AsQueryable();
}
