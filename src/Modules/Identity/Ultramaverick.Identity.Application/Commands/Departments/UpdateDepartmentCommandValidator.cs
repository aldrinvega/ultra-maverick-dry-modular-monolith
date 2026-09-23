using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.Departments
{
    public sealed class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentCommandValidator()
        {
            RuleFor(x => x.DepartmentId).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        }
    }
}
