using Microsoft.EntityFrameworkCore;
using PayCore.Core.Entities;
using PayCore.Core.Interfaces;
using PayCore.Infrastructure.Data;

namespace PayCore.Infrastructure.Repositories;

public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Department>> GetAllWithManagerAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking()
            .Include(d => d.Manager)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

    public async Task<Department?> GetByIdWithManagerAsync(Guid id, CancellationToken ct = default)
        => await _dbSet
            .Include(d => d.Manager)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
}
