using MediatR;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Application.Models;

namespace Ultramaverick.Quality.Application.Commands.QcReceipts;

public sealed class RejectQcReceiptCommandHandler : IRequestHandler<RejectQcReceiptCommand, Result>
{
    private readonly IQualityUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public RejectQcReceiptCommandHandler(IQualityUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(RejectQcReceiptCommand request, CancellationToken ct)
    {
        var receipt = await _uow.QcReceipts.GetByIdAsync(request.QcReceiptId, ct);
        if (receipt is null)
            return Result.Failure($"QC receipt '{request.QcReceiptId}' does not exist.");

        try
        {
            receipt.Reject(request.Quantity, request.ReasonId, request.Remarks, _currentUser.UserId);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
