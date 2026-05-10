namespace PayCore.Core.DTOs.Departments;

public record CreateDepartmentRequest(string Name, Guid? ManagerId);
