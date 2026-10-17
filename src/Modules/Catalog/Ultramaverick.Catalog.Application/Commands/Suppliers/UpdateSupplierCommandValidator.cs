using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Suppliers;

public sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.SupplierCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SupplierName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SupplierAddress).MaximumLength(300);
    }
}