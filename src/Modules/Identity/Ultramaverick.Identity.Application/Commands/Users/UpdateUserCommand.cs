using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Users
{
    public sealed record UpdateUserCommand(
        int UserId, 
        string FullName, 
        string UserName, 
        string Password, 
        int RoleId, 
        int DepartmentId)
         : IRequest<Result>;

}
