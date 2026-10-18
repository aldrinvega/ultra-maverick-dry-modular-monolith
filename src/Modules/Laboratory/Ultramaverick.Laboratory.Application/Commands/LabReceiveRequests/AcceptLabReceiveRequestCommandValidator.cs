using FluentValidation;

namespace Ultramaverick.Laboratory.Application.Commands.LabReceiveRequests;

public sealed class AcceptLabReceiveRequestCommandValidator
    : AbstractValidator<AcceptLabReceiveRequestCommand>
{
    public AcceptLabReceiveRequestCommandValidator()
    {
        RuleFor(x => x.LabTestRequestId).GreaterThan(0);
    }
}
