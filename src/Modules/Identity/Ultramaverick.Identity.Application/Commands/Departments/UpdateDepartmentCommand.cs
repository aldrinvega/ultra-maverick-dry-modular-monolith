using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Departments
{
    public sealed record UpdateDepartmentCommand(int DepartmentId, string Name) : IRequest<Result>;
}
