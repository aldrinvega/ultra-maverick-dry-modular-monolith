using MediatR;
using Ultramaverick.Catalog.Application.Abstractions;
using Ultramaverick.Catalog.Application.Models;
using Ultramaverick.Catalog.Domain.Entities;

namespace Ultramaverick.Catalog.Application.Commands.Reasons;

public sealed class CreateReasonCommandHandler : IRequestHandler<CreateReasonCommand, Result<int>>
{
    private readonly ICatalogUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public CreateReasonCommandHandler(ICatalogUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CreateReasonCommand request, CancellationToken ct)
    {
        // Reason names are scoped to a menu, so they are not unique on their own.
        var name = (request.Name ?? string.Empty).Trim();

        var reason = Reason.Create(name, request.MenuId, _currentUser.UserId);

        await _uow.Reasons.AddAsync(reason, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(reason.Id);
    }
}