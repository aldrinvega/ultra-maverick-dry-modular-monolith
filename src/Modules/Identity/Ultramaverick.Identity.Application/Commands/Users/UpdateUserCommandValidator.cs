using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ultramaverick.Identity.Application.Commands.Users
{
    public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.RoleId).GreaterThan(0);
            RuleFor(x => x.DepartmentId).GreaterThan(0);
        }
    }
}
