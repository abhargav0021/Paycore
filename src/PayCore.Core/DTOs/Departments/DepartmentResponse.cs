namespace PayCore.Core.DTOs.Departments;

public record DepartmentResponse(Guid Id, string Name, Guid? ManagerId, string? ManagerName);
