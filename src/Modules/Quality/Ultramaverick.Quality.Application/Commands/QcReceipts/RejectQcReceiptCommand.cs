using MediatR;
using Ultramaverick.Quality.Application.Models;

namespace Ultramaverick.Quality.Application.Commands.QcReceipts;

public sealed record RejectQcReceiptCommand(
    int QcReceiptId,
    decimal Quantity,
    int? ReasonId,
    string? Remarks) : IRequest<Result>;
