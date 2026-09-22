using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Users
{
    public sealed record CreateUserCommand(string FullName, string UserName, string Password, int RoleId, int DepartmentId)
            : IRequest<Result<int>>;
}
