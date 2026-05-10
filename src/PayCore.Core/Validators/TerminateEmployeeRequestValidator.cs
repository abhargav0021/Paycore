using FluentValidation;
using PayCore.Core.DTOs.Employees;

namespace PayCore.Core.Validators;

public class TerminateEmployeeRequestValidator : AbstractValidator<TerminateEmployeeRequest>
{
    public TerminateEmployeeRequestValidator()
    {
        RuleFor(x => x.TerminationDate).NotEmpty();
    }
}
