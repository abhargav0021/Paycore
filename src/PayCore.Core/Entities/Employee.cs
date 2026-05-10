using PayCore.Core.Enums;

namespace PayCore.Core.Entities;

public class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmploymentType EmploymentType { get; set; }
    public decimal? Salary { get; set; }
    public decimal? HourlyRate { get; set; }
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }
}
