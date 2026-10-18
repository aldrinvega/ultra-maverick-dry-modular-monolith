using FluentValidation.TestHelper;
using Ultramaverick.Quality.Application.Commands.QcReceipts;

namespace Ultramaverick.Quality.UnitTests.Application;

public class RejectQcReceiptCommandValidatorTests
{
    private readonly RejectQcReceiptCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.TestValidate(new RejectQcReceiptCommand(1, 2m, null, "damaged"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_receipt_id_fails_on_QcReceiptId(int id)
    {
        var result = _validator.TestValidate(new RejectQcReceiptCommand(id, 1m, null, null));

        Assert.False(result.IsValid);
        Assert.Equal("QcReceiptId", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_quantity_fails_on_Quantity(decimal quantity)
    {
        var result = _validator.TestValidate(new RejectQcReceiptCommand(1, quantity, null, null));

        Assert.False(result.IsValid);
        Assert.Equal("Quantity", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Quantity_with_more_than_four_decimal_places_fails_on_Quantity()
    {
        var result = _validator.TestValidate(new RejectQcReceiptCommand(1, 1.00001m, null, null));

        Assert.False(result.IsValid);
        Assert.Equal("Quantity", Assert.Single(result.Errors).PropertyName);
    }
}
