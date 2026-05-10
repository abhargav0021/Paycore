using PayCore.Core.DTOs.Common;
using PayCore.Core.DTOs.Employees;
using PayCore.Core.Entities;
using PayCore.Core.Interfaces;

namespace PayCore.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IDepartmentRepository _deptRepo;

    public EmployeeService(IEmployeeRepository employeeRepo, IDepartmentRepository deptRepo)
    {
        _employeeRepo = employeeRepo;
        _deptRepo = deptRepo;
    }

    public async Task<PagedResult<EmployeeResponse>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _employeeRepo.GetPagedAsync(page, pageSize, ct);
        return new PagedResult<EmployeeResponse>(
            items.Select(MapToResponse).ToList(), page, pageSize, totalCount);
    }

    public async Task<EmployeeResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdWithIncludesAsync(id, ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");
        return MapToResponse(employee);
    }

    public async Task<EmployeeResponse> HireAsync(HireEmployeeRequest request, CancellationToken ct = default)
    {
        _ = await _deptRepo.GetByIdAsync(request.DepartmentId, ct)
            ?? throw new KeyNotFoundException($"Department {request.DepartmentId} not found.");

        if (request.ManagerId.HasValue)
            _ = await _employeeRepo.GetByIdAsync(request.ManagerId.Value, ct)
                ?? throw new KeyNotFoundException($"Manager {request.ManagerId.Value} not found.");

        if (await _employeeRepo.ExistsByEmailAsync(request.Email, ct))
            throw new InvalidOperationException($"An employee with email '{request.Email}' already exists.");

        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            HireDate = request.HireDate,
            EmploymentType = request.EmploymentType,
            Salary = request.Salary,
            HourlyRate = request.HourlyRate,
            DepartmentId = request.DepartmentId,
            ManagerId = request.ManagerId
        };

        await _employeeRepo.AddAsync(employee, ct);
        await _employeeRepo.SaveChangesAsync(ct);

        var created = await _employeeRepo.GetByIdWithIncludesAsync(employee.Id, ct);
        return MapToResponse(created!);
    }

    public async Task<EmployeeResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        _ = await _deptRepo.GetByIdAsync(request.DepartmentId, ct)
            ?? throw new KeyNotFoundException($"Department {request.DepartmentId} not found.");

        if (request.ManagerId.HasValue)
            _ = await _employeeRepo.GetByIdAsync(request.ManagerId.Value, ct)
                ?? throw new KeyNotFoundException($"Manager {request.ManagerId.Value} not found.");

        if (!string.Equals(employee.Email, request.Email, StringComparison.OrdinalIgnoreCase)
            && await _employeeRepo.ExistsByEmailAsync(request.Email, ct))
            throw new InvalidOperationException($"An employee with email '{request.Email}' already exists.");

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Phone = request.Phone;
        employee.EmploymentType = request.EmploymentType;
        employee.Salary = request.Salary;
        employee.HourlyRate = request.HourlyRate;
        employee.DepartmentId = request.DepartmentId;
        employee.ManagerId = request.ManagerId;
        employee.UpdatedAt = DateTime.UtcNow;

        _employeeRepo.Update(employee);
        await _employeeRepo.SaveChangesAsync(ct);

        var updated = await _employeeRepo.GetByIdWithIncludesAsync(id, ct);
        return MapToResponse(updated!);
    }

    public async Task TerminateAsync(Guid id, TerminateEmployeeRequest request, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        employee.TerminationDate = request.TerminationDate;
        employee.UpdatedAt = DateTime.UtcNow;

        _employeeRepo.Update(employee);
        await _employeeRepo.SaveChangesAsync(ct);
    }

    private static EmployeeResponse MapToResponse(Employee e) => new(
        e.Id,
        e.FirstName,
        e.LastName,
        e.Email,
        e.Phone,
        e.HireDate,
        e.TerminationDate,
        e.EmploymentType,
        e.Salary,
        e.HourlyRate,
        e.DepartmentId,
        e.Department?.Name ?? string.Empty,
        e.ManagerId,
        e.Manager is null ? null : $"{e.Manager.FirstName} {e.Manager.LastName}"
    );
}
