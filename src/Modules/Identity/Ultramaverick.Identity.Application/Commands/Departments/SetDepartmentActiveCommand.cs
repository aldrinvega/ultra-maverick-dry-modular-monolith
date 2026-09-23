using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Departments
{
    public sealed record SetDepartmentActiveCommand(int DepartmentId, bool IsActive) : IRequest<Result>;
}
