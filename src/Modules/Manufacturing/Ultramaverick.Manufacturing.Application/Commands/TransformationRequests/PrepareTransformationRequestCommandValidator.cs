using FluentValidation;

namespace Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;

public sealed class PrepareTransformationRequestCommandValidator
    : AbstractValidator<PrepareTransformationRequestCommand>
{
    public PrepareTransformationRequestCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
