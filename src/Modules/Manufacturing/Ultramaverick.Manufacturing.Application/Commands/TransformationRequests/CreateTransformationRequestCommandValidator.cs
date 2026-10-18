using FluentValidation;

namespace Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;

public sealed class CreateTransformationRequestCommandValidator
    : AbstractValidator<CreateTransformationRequestCommand>
{
    public CreateTransformationRequestCommandValidator()
    {
        RuleFor(x => x.ProductionPlanId).GreaterThan(0);
        RuleFor(x => x.WarehouseId).GreaterThan(0);
        RuleFor(x => x.Lines).NotEmpty();

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId).GreaterThan(0);
            line.RuleFor(l => l.RequiredQuantity)
                .GreaterThan(0)
                .PrecisionScale(18, 4, ignoreTrailingZeros: false);
        });
    }
}
