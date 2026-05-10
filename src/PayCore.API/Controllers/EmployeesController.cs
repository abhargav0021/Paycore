using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayCore.Core.DTOs.Common;
using PayCore.Core.DTOs.Employees;
using PayCore.Core.Interfaces;

namespace PayCore.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService) => _employeeService = employeeService;

    /// <summary>Paginated employee list. Admin and Manager only.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(PagedResult<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _employeeService.GetPagedAsync(page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Get one employee. Admin/Manager can view any; Employee role can only view their own record.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (User.IsInRole("Employee"))
        {
            var claimedId = User.FindFirstValue("employeeId");
            if (claimedId != id.ToString())
                return Forbid();
        }

        var employee = await _employeeService.GetByIdAsync(id, ct);
        return Ok(employee);
    }

    /// <summary>Hire a new employee. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Hire(HireEmployeeRequest request, CancellationToken ct)
    {
        var employee = await _employeeService.HireAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    /// <summary>Update employee details. Admin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateEmployeeRequest request, CancellationToken ct)
    {
        var employee = await _employeeService.UpdateAsync(id, request, ct);
        return Ok(employee);
    }

    /// <summary>Soft-delete an employee by setting their termination date. Admin only.</summary>
    [HttpPost("{id:guid}/terminate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Terminate(Guid id, TerminateEmployeeRequest request, CancellationToken ct)
    {
        await _employeeService.TerminateAsync(id, request, ct);
        return NoContent();
    }
}
