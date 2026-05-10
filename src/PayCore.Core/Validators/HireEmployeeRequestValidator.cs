using FluentValidation;
using PayCore.Core.DTOs.Employees;

namespace PayCore.Core.Validators;

public class HireEmployeeRequestValidator : AbstractValidator<HireEmployeeRequest>
{
    public HireEmployeeRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => x.Phone is not null);
        RuleFor(x => x.HireDate).NotEmpty();
        RuleFor(x => x.EmploymentType).IsInEnum();
        RuleFor(x => x.Salary).GreaterThan(0).When(x => x.Salary.HasValue);
        RuleFor(x => x.HourlyRate).GreaterThan(0).When(x => x.HourlyRate.HasValue);
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}
