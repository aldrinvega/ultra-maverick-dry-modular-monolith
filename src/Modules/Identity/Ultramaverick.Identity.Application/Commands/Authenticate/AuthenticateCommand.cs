using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Authenticate
{
    public sealed record AuthenticateCommand(string UserName, string Password) : IRequest<Result<AuthenticateResponse>>;
}
