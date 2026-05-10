using PayCore.Core.Enums;

namespace PayCore.Core.DTOs.Employees;

public record HireEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime HireDate,
    EmploymentType EmploymentType,
    decimal? Salary,
    decimal? HourlyRate,
    Guid DepartmentId,
    Guid? ManagerId
);
