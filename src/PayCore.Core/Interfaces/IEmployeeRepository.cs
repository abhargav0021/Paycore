using PayCore.Core.Entities;

namespace PayCore.Core.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default);
    Task<Employee?> GetByIdWithIncludesAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}
