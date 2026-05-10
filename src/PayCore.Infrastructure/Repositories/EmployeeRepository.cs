using Microsoft.EntityFrameworkCore;
using PayCore.Core.Entities;
using PayCore.Core.Interfaces;
using PayCore.Infrastructure.Data;

namespace PayCore.Infrastructure.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Manager);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Employee?> GetByIdWithIncludesAsync(Guid id, CancellationToken ct = default)
        => await _dbSet
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await _dbSet.AnyAsync(e => e.Email == email, ct);
}
