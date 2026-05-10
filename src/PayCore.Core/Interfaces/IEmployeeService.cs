using PayCore.Core.DTOs.Common;
using PayCore.Core.DTOs.Employees;

namespace PayCore.Core.Interfaces;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeResponse>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<EmployeeResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EmployeeResponse> HireAsync(HireEmployeeRequest request, CancellationToken ct = default);
    Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task TerminateAsync(Guid id, TerminateEmployeeRequest request, CancellationToken ct = default);
}
