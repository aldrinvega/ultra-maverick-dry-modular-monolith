using MediatR;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Application.Models;

namespace Ultramaverick.Quality.Application.Queries.QcReceipts;

public sealed record QcChecklistAnswerDto(
    string Question,
    bool Status,
    string? Remarks);

public sealed record QcReceiptDetailDto(
    int Id,
    int PurchaseOrderId,
    int? PurchaseOrderLineId,
    int ItemId,
    int UomId,
    int? ProductTypeId,
    string? BatchNo,
    DateTime? ManufacturingDate,
    DateTime? ExpiryDate,
    decimal ActualDelivered,
    decimal TotalReject,
    bool IsNearlyExpire,
    bool IsWarehouseReceived,
    string Status,
    bool IsActive,
    IReadOnlyList<QcChecklistAnswerDto> ChecklistAnswers);

public sealed record GetQcReceiptByIdQuery(int QcReceiptId)
    : IRequest<Result<QcReceiptDetailDto>>;

public sealed class GetQcReceiptByIdQueryHandler
    : IRequestHandler<GetQcReceiptByIdQuery, Result<QcReceiptDetailDto>>
{
    private readonly IQcReceiptReader _reader;

    public GetQcReceiptByIdQueryHandler(IQcReceiptReader reader) => _reader = reader;

    public async Task<Result<QcReceiptDetailDto>> Handle(GetQcReceiptByIdQuery request, CancellationToken ct)
    {
        if (request.QcReceiptId <= 0)
            return Result<QcReceiptDetailDto>.Failure("QC receipt id must be greater than zero.");

        var receipt = await _reader.GetByIdAsync(request.QcReceiptId, ct);

        return receipt is null
            ? Result<QcReceiptDetailDto>.Failure("QC receipt not found.")
            : Result<QcReceiptDetailDto>.Success(receipt);
    }
}
