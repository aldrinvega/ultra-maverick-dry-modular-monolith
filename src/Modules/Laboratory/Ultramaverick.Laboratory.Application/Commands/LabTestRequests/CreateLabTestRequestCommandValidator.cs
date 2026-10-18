using FluentValidation;

namespace Ultramaverick.Laboratory.Application.Commands.LabTestRequests;

public sealed class CreateLabTestRequestCommandValidator
    : AbstractValidator<CreateLabTestRequestCommand>
{
    public CreateLabTestRequestCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).PrecisionScale(18, 4, ignoreTrailingZeros: false);
        RuleFor(x => x.WarehouseReceiptId).GreaterThan(0).When(x => x.WarehouseReceiptId.HasValue);
        RuleFor(x => x.TestType).MaximumLength(100);
        RuleFor(x => x.BatchNo).MaximumLength(100);
    }
}
