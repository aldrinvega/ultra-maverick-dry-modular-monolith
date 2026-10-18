using FluentValidation;

namespace Ultramaverick.Laboratory.Application.Commands.LabTestResults;

public sealed class RecordLabResultCommandValidator : AbstractValidator<RecordLabResultCommand>
{
    public RecordLabResultCommandValidator()
    {
        RuleFor(x => x.LabReceiveRequestId).GreaterThan(0);
        RuleFor(x => x.AnalyzedAt).NotEmpty();
        RuleFor(x => x.Result).NotEmpty().MaximumLength(2000);
    }
}
