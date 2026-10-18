using FluentValidation;

namespace Ultramaverick.Quality.Application.Commands.QcReceipts;

public sealed class RejectQcReceiptCommandValidator : AbstractValidator<RejectQcReceiptCommand>
{
    public RejectQcReceiptCommandValidator()
    {
        RuleFor(x => x.QcReceiptId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0).PrecisionScale(18, 4, ignoreTrailingZeros: false);
    }
}
