using MediatR;
using Ultramaverick.Identity.Application.Abstractions;
using Ultramaverick.Identity.Application.Models;
using Ultramaverick.Identity.Domain.Entities;

namespace Ultramaverick.Identity.Application.Commands.Modules
{
    public sealed class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, Result<int>>
    {
        private readonly IModuleRepository _modules;
        private readonly IIdentityUnitOfWork _uow;

        public CreateModuleCommandHandler(IModuleRepository modules, IIdentityUnitOfWork uow)
        {
            _modules = modules;
            _uow = uow;
        }

        public async Task<Result<int>> Handle(CreateModuleCommand request, CancellationToken ct)
        {
            var module = Module.Create(request.MainMenuId, request.Name, request.SubMenuName);

            await _modules.AddAsync(module, ct);
            await _uow.SaveChangesAsync(ct);   // shares the scoped DbContext

            return Result<int>.Success(module.Id);
        }
    }
}
