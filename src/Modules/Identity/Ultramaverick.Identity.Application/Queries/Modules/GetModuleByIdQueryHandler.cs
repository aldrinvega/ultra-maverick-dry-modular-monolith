using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Identity.Application.Queries.Modules
{
    public sealed class GetModuleByIdQueryHandler : IRequestHandler<GetModuleByIdQuery, Result<ModuleDto>>
    {
        private readonly IModuleRepository _modules;

        public GetModuleByIdQueryHandler(IModuleRepository modules) => _modules = modules;

        public async Task<Result<ModuleDto>> Handle(GetModuleByIdQuery request, CancellationToken ct)
        {
            var module = await _modules.GetByIdAsync(request.ModuleId, ct);
            if (module is null) return Result<ModuleDto>.Failure("Module not found.");

            return Result<ModuleDto>.Success(new ModuleDto(module.Id, module.Name, module.IsActive));
        }
    }
}
