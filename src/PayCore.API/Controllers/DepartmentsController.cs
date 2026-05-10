using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayCore.Core.DTOs.Departments;
using PayCore.Core.Interfaces;

namespace PayCore.API.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService) => _departmentService = departmentService;

    /// <summary>List all departments. All authenticated roles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var departments = await _departmentService.GetAllAsync(ct);
        return Ok(departments);
    }

    /// <summary>Create a department. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreateDepartmentRequest request, CancellationToken ct)
    {
        var department = await _departmentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), department);
    }
}
