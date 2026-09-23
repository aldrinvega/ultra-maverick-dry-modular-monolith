using FluentValidation;

namespace Ultramaverick.Identity.Application.Commands.Departments
{
    public sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        }
    }
}
