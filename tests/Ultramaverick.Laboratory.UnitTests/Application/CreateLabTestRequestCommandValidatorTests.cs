using FluentValidation.TestHelper;
using Ultramaverick.Laboratory.Application.Commands.LabTestRequests;

namespace Ultramaverick.Laboratory.UnitTests.Application;

public class CreateLabTestRequestCommandValidatorTests
{
    private readonly CreateLabTestRequestCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.TestValidate(Command(quantity: 2m));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_quantity_fails_on_Quantity(decimal quantity)
    {
        var result = _validator.TestValidate(Command(quantity: quantity));

        Assert.False(result.IsValid);
        Assert.Equal("Quantity", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Quantity_with_more_than_four_decimal_places_fails_on_Quantity()
    {
        var result = _validator.TestValidate(Command(quantity: 1.00001m));

        Assert.False(result.IsValid);
        Assert.Equal("Quantity", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_warehouse_receipt_id_fails_on_WarehouseReceiptId(int id)
    {
        var result = _validator.TestValidate(Command(warehouseReceiptId: id));

        Assert.False(result.IsValid);
        Assert.Equal("WarehouseReceiptId", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Missing_warehouse_receipt_id_is_allowed()
    {
        var result = _validator.TestValidate(Command(warehouseReceiptId: null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Overlong_test_type_fails_on_TestType()
    {
        var result = _validator.TestValidate(Command(testType: new string('x', 101)));

        Assert.False(result.IsValid);
        Assert.Equal("TestType", Assert.Single(result.Errors).PropertyName);
    }

    private static CreateLabTestRequestCommand Command(
        int? warehouseReceiptId = 1,
        decimal quantity = 1m,
        string? testType = "Micro")
        => new(warehouseReceiptId, "LOT-1", quantity, testType, null, null,
            null, null, null, null, null);
}
