using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Modules
{
    public sealed record GetModuleByIdQuery(int ModuleId) : IRequest<Result<ModuleDto>>;
}
