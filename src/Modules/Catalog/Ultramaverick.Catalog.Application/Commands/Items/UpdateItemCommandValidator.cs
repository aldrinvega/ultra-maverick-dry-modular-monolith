using FluentValidation;

namespace Ultramaverick.Catalog.Application.Commands.Items;

public sealed class UpdateItemCommandValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.ItemDescription).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ItemCategoryId).GreaterThan(0);
        RuleFor(x => x.UomId).GreaterThan(0);
        RuleFor(x => x.BufferLevel).GreaterThanOrEqualTo(0).When(x => x.BufferLevel.HasValue);
    }
}