using PayCore.Core.Enums;

namespace PayCore.Core.DTOs.Employees;

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    EmploymentType EmploymentType,
    decimal? Salary,
    decimal? HourlyRate,
    Guid DepartmentId,
    Guid? ManagerId
);
