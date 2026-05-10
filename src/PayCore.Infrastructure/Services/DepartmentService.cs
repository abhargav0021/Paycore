using PayCore.Core.DTOs.Departments;
using PayCore.Core.Entities;
using PayCore.Core.Interfaces;

namespace PayCore.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _deptRepo;
    private readonly IEmployeeRepository _employeeRepo;

    public DepartmentService(IDepartmentRepository deptRepo, IEmployeeRepository employeeRepo)
    {
        _deptRepo = deptRepo;
        _employeeRepo = employeeRepo;
    }

    public async Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var departments = await _deptRepo.GetAllWithManagerAsync(ct);
        return departments.Select(MapToResponse).ToList();
    }

    public async Task<DepartmentResponse> CreateAsync(CreateDepartmentRequest request, CancellationToken ct = default)
    {
        if (request.ManagerId.HasValue)
            _ = await _employeeRepo.GetByIdAsync(request.ManagerId.Value, ct)
                ?? throw new KeyNotFoundException($"Manager {request.ManagerId.Value} not found.");

        var department = new Department
        {
            Name = request.Name,
            ManagerId = request.ManagerId
        };

        await _deptRepo.AddAsync(department, ct);
        await _deptRepo.SaveChangesAsync(ct);

        var created = await _deptRepo.GetByIdWithManagerAsync(department.Id, ct);
        return MapToResponse(created!);
    }

    private static DepartmentResponse MapToResponse(Department d) => new(
        d.Id,
        d.Name,
        d.ManagerId,
        d.Manager is null ? null : $"{d.Manager.FirstName} {d.Manager.LastName}"
    );
}
