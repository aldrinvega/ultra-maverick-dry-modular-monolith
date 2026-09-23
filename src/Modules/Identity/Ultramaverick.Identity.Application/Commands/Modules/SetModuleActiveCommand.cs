using MediatR;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Commands.Modules
{
    public sealed record SetModuleActiveCommand(int ModuleId, bool IsActive) : IRequest<Result>;
}
