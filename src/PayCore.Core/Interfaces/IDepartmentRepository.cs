using PayCore.Core.Entities;

namespace PayCore.Core.Interfaces;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<IReadOnlyList<Department>> GetAllWithManagerAsync(CancellationToken ct = default);
    Task<Department?> GetByIdWithManagerAsync(Guid id, CancellationToken ct = default);
}
