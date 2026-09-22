using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries
{
    public sealed record GetUserByIdQuery(int UserId) : IRequest<Result<UserDto>>;
}
