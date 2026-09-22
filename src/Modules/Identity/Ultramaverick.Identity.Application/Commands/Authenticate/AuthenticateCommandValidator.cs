using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ultramaverick.Identity.Application.Commands.Authenticate
{
    public sealed class AuthenticateCommandValidator : AbstractValidator<AuthenticateCommand>
    {
        public AuthenticateCommandValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
        }
    }
}
