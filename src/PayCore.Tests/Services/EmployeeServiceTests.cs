using Moq;
using PayCore.Core.DTOs.Employees;
using PayCore.Core.Entities;
using PayCore.Core.Enums;
using PayCore.Core.Interfaces;
using PayCore.Infrastructure.Services;
using Xunit;

namespace PayCore.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepo = new();
    private readonly Mock<IDepartmentRepository> _deptRepo = new();
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
        => _sut = new EmployeeService(_employeeRepo.Object, _deptRepo.Object);

    // ── GetPagedAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPageMetadata()
    {
        var dept = new Department { Id = Guid.NewGuid(), Name = "Eng" };
        var employees = Enumerable.Range(1, 5)
            .Select(i => new Employee
            {
                Id = Guid.NewGuid(), FirstName = $"F{i}", LastName = $"L{i}",
                Email = $"e{i}@test.com", DepartmentId = dept.Id, Department = dept
            })
            .ToList();

        _employeeRepo
            .Setup(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<Employee>)employees, 25));

        var result = await _sut.GetPagedAsync(1, 10);

        Assert.Equal(5, result.Items.Count);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalPages);
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsEmployee_WhenFound()
    {
        var id = Guid.NewGuid();
        var dept = new Department { Id = Guid.NewGuid(), Name = "Finance" };
        var employee = new Employee
        {
            Id = id, FirstName = "Alice", LastName = "Smith",
            Email = "alice@test.com", DepartmentId = dept.Id, Department = dept
        };

        _employeeRepo
            .Setup(r => r.GetByIdWithIncludesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var result = await _sut.GetByIdAsync(id);

        Assert.Equal("Alice", result.FirstName);
        Assert.Equal("Smith", result.LastName);
        Assert.Equal("Finance", result.DepartmentName);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        _employeeRepo
            .Setup(r => r.GetByIdWithIncludesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid()));
    }

    // ── HireAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task HireAsync_CreatesEmployee_WhenValid()
    {
        var deptId = Guid.NewGuid();
        var dept = new Department { Id = deptId, Name = "Engineering" };
        var request = new HireEmployeeRequest(
            "John", "Doe", "john@test.com", null,
            DateTime.UtcNow.AddDays(-30), EmploymentType.FullTime, 80_000m, null, deptId, null);

        Employee? saved = null;

        _deptRepo.Setup(r => r.GetByIdAsync(deptId, It.IsAny<CancellationToken>())).ReturnsAsync(dept);
        _employeeRepo.Setup(r => r.ExistsByEmailAsync("john@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepo
            .Setup(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()))
            .Callback<Employee, CancellationToken>((e, _) => { saved = e; e.Department = dept; })
            .Returns(Task.CompletedTask);
        _employeeRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _employeeRepo
            .Setup(r => r.GetByIdWithIncludesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => saved);

        var result = await _sut.HireAsync(request);

        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@test.com", result.Email);
        Assert.Equal("Engineering", result.DepartmentName);
        _employeeRepo.Verify(r => r.AddAsync(It.IsAny<Employee>(), It.IsAny<CancellationToken>()), Times.Once);
        _employeeRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HireAsync_ThrowsKeyNotFoundException_WhenDepartmentNotFound()
    {
        var request = new HireEmployeeRequest(
            "Jane", "Doe", "jane@test.com", null,
            DateTime.UtcNow, EmploymentType.FullTime, 70_000m, null, Guid.NewGuid(), null);

        _deptRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.HireAsync(request));
    }

    [Fact]
    public async Task HireAsync_ThrowsKeyNotFoundException_WhenManagerNotFound()
    {
        var deptId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var request = new HireEmployeeRequest(
            "Bob", "Brown", "bob@test.com", null,
            DateTime.UtcNow, EmploymentType.PartTime, null, 50m, deptId, managerId);

        _deptRepo.Setup(r => r.GetByIdAsync(deptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { Id = deptId, Name = "Ops" });
        _employeeRepo.Setup(r => r.GetByIdAsync(managerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.HireAsync(request));
    }

    [Fact]
    public async Task HireAsync_ThrowsInvalidOperationException_WhenEmailAlreadyExists()
    {
        var deptId = Guid.NewGuid();
        var request = new HireEmployeeRequest(
            "Eve", "Adams", "existing@test.com", null,
            DateTime.UtcNow, EmploymentType.Contractor, null, 75m, deptId, null);

        _deptRepo.Setup(r => r.GetByIdAsync(deptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { Id = deptId, Name = "Legal" });
        _employeeRepo.Setup(r => r.ExistsByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.HireAsync(request));
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_UpdatesEmployee_WhenValid()
    {
        var id = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        var dept = new Department { Id = deptId, Name = "HR" };
        var existing = new Employee
        {
            Id = id, FirstName = "Old", LastName = "Name",
            Email = "old@test.com", DepartmentId = deptId
        };
        var updated = new Employee
        {
            Id = id, FirstName = "New", LastName = "Name",
            Email = "new@test.com", DepartmentId = deptId, Department = dept
        };
        var request = new UpdateEmployeeRequest(
            "New", "Name", "new@test.com", null,
            EmploymentType.FullTime, 90_000m, null, deptId, null);

        _employeeRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _deptRepo.Setup(r => r.GetByIdAsync(deptId, It.IsAny<CancellationToken>())).ReturnsAsync(dept);
        _employeeRepo.Setup(r => r.ExistsByEmailAsync("new@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _employeeRepo.Setup(r => r.Update(It.IsAny<Employee>()));
        _employeeRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _employeeRepo.Setup(r => r.GetByIdWithIncludesAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var result = await _sut.UpdateAsync(id, request);

        Assert.Equal("New", result.FirstName);
        Assert.Equal("new@test.com", result.Email);
        _employeeRepo.Verify(r => r.Update(It.IsAny<Employee>()), Times.Once);
        _employeeRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenEmployeeNotFound()
    {
        _employeeRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var request = new UpdateEmployeeRequest(
            "X", "Y", "x@test.com", null, EmploymentType.FullTime, null, null, Guid.NewGuid(), null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenDepartmentNotFound()
    {
        var id = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        _employeeRepo
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = id, Email = "e@test.com" });
        _deptRepo
            .Setup(r => r.GetByIdAsync(deptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Department?)null);

        var request = new UpdateEmployeeRequest(
            "X", "Y", "e@test.com", null, EmploymentType.FullTime, null, null, deptId, null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(id, request));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperationException_WhenEmailTakenByAnotherEmployee()
    {
        var id = Guid.NewGuid();
        var deptId = Guid.NewGuid();
        _employeeRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee { Id = id, Email = "original@test.com" });
        _deptRepo.Setup(r => r.GetByIdAsync(deptId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { Id = deptId, Name = "IT" });
        _employeeRepo.Setup(r => r.ExistsByEmailAsync("taken@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new UpdateEmployeeRequest(
            "X", "Y", "taken@test.com", null, EmploymentType.FullTime, null, null, deptId, null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(id, request));
    }

    // ── TerminateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task TerminateAsync_SetsTerminationDate_WhenFound()
    {
        var id = Guid.NewGuid();
        var employee = new Employee { Id = id, Email = "emp@test.com" };
        var terminationDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        _employeeRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);
        _employeeRepo.Setup(r => r.Update(It.IsAny<Employee>()));
        _employeeRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.TerminateAsync(id, new TerminateEmployeeRequest(terminationDate));

        Assert.Equal(terminationDate, employee.TerminationDate);
        _employeeRepo.Verify(r => r.Update(employee), Times.Once);
        _employeeRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TerminateAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        _employeeRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.TerminateAsync(Guid.NewGuid(), new TerminateEmployeeRequest(DateTime.UtcNow)));
    }
}
