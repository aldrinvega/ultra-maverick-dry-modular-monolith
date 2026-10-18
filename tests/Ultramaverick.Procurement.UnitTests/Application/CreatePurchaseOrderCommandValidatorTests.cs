using FluentValidation.TestHelper;
using Ultramaverick.Procurement.Application.Commands.PurchaseOrders;

namespace Ultramaverick.Procurement.UnitTests.Application;

public class CreatePurchaseOrderCommandValidatorTests
{
    private readonly CreatePurchaseOrderCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.TestValidate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_po_number_fails_on_PoNumber(string poNumber)
    {
        var result = _validator.TestValidate(ValidCommand(poNumber));

        Assert.False(result.IsValid);
        Assert.Equal("PoNumber", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Po_number_longer_than_50_chars_fails_on_PoNumber()
    {
        var result = _validator.TestValidate(ValidCommand(new string('X', 51)));

        Assert.False(result.IsValid);
        Assert.Equal("PoNumber", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_supplier_fails_on_SupplierId(int supplierId)
    {
        var result = _validator.TestValidate(ValidCommand(supplierId: supplierId));

        Assert.False(result.IsValid);
        Assert.Equal("SupplierId", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Empty_lines_fail_on_Lines()
    {
        var result = _validator.TestValidate(ValidCommand(lines: Array.Empty<PurchaseOrderLineInput>()));

        Assert.False(result.IsValid);
        Assert.Equal("Lines", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Null_line_entry_fails_on_Lines()
    {
        var result = _validator.TestValidate(ValidCommand(lines: new List<PurchaseOrderLineInput> { null! }));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.StartsWith("Lines", StringComparison.Ordinal));
    }

    [Fact]
    public void Five_hundred_lines_pass()
    {
        var result = _validator.TestValidate(ValidCommand(lines: Lines(500)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Five_hundred_and_one_lines_fail_on_Lines()
    {
        var result = _validator.TestValidate(ValidCommand(lines: Lines(501)));

        Assert.False(result.IsValid);
        Assert.Equal("Lines", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_quantity_fails_on_Quantity(decimal quantity)
    {
        var result = _validator.TestValidate(ValidCommand(lines: Lines(1, quantity: quantity)));

        Assert.False(result.IsValid);
        Assert.Equal("Lines[0].Quantity", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Quantity_with_more_than_four_decimal_places_fails()
    {
        var result = _validator.TestValidate(ValidCommand(lines: Lines(1, quantity: 1.00001m)));

        Assert.False(result.IsValid);
        Assert.Equal("Lines[0].Quantity", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Unit_price_with_more_than_four_decimal_places_fails()
    {
        var result = _validator.TestValidate(ValidCommand(lines: Lines(1, unitPrice: 1.00001m)));

        Assert.False(result.IsValid);
        Assert.Equal("Lines[0].UnitPrice", Assert.Single(result.Errors).PropertyName);
    }

    private static CreatePurchaseOrderCommand ValidCommand(
        string poNumber = "PO-1", int supplierId = 1, IReadOnlyList<PurchaseOrderLineInput>? lines = null)
        => new(poNumber, null, null, null, supplierId,
            lines ?? new[] { new PurchaseOrderLineInput(10, 20, 1m, 1m) });

    private static PurchaseOrderLineInput[] Lines(int count, decimal quantity = 1m, decimal unitPrice = 1m)
        => Enumerable.Range(1, count)
            .Select(i => new PurchaseOrderLineInput(i, i, quantity, unitPrice))
            .ToArray();
}
