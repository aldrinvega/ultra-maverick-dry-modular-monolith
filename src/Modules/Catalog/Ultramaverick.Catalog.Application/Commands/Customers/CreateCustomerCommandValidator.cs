using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Customers;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FarmId).GreaterThan(0).When(x => x.FarmId.HasValue);
        RuleFor(x => x.AccountTitleId).GreaterThan(0).When(x => x.AccountTitleId.HasValue);
        RuleFor(x => x.LocationId).GreaterThan(0).When(x => x.LocationId.HasValue);
        RuleFor(x => x.DepartmentId).GreaterThan(0).When(x => x.DepartmentId.HasValue);
        RuleFor(x => x.CompanyCode).MaximumLength(50);
        RuleFor(x => x.CompanyName).MaximumLength(200);
        RuleFor(x => x.MobileNumber).MaximumLength(30);
        RuleFor(x => x.LeadMan).MaximumLength(150);
        RuleFor(x => x.Address).MaximumLength(300);
    }
}