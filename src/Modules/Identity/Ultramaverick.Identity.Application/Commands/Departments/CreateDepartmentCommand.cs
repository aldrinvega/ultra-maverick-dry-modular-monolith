using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Departments
{
    public sealed record CreateDepartmentCommand(string Name) : IRequest<Result<int>>;
}
