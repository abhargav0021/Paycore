using PayCore.Core.DTOs.Departments;

namespace PayCore.Core.Interfaces;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(CancellationToken ct = default);
    Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken ct = default);
}
