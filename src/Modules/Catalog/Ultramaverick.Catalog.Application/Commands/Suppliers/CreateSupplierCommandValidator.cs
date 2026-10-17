using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Suppliers;

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.SupplierCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SupplierName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SupplierAddress).MaximumLength(300);
    }
}