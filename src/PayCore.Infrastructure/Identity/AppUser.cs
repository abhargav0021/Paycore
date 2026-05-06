using Microsoft.AspNetCore.Identity;

namespace PayCore.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public string? DepartmentId { get; set; }
    public string? EmployeeId { get; set; }
}
