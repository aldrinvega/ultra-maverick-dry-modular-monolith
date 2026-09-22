using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ultramaverick.Identity.Application.Commands.Users
{
    public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(256);
            RuleFor(x => x.RoleId).GreaterThan(0);
            RuleFor(x => x.DepartmentId).GreaterThan(0);
        }
    }
}
