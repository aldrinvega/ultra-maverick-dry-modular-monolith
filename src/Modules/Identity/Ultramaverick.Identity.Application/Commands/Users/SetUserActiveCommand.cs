using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Users
{
    public sealed record SetUserActiveCommand(int UserId, bool IsActive) : IRequest<Result>;

}
