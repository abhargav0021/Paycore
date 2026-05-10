using PayCore.Core.Enums;

namespace PayCore.Core.DTOs.Employees;

public record EmployeeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime HireDate,
    DateTime? TerminationDate,
    EmploymentType EmploymentType,
    decimal? Salary,
    decimal? HourlyRate,
    Guid DepartmentId,
    string DepartmentName,
    Guid? ManagerId,
    string? ManagerName
);
