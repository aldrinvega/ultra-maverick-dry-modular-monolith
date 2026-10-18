using Ultramaverick.Warehouse.Application.Commands.Warehouses;

namespace Ultramaverick.Warehouse.UnitTests.Application;

public class CreateWarehouseCommandValidatorTests
{
    private readonly CreateWarehouseCommandValidator _validator = new();

    [Fact]
    public void A_well_formed_command_is_valid()
        => Assert.True(_validator.Validate(
            new CreateWarehouseCommand("WH-1", "Main", 3, null)).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void A_blank_code_is_invalid(string code)
        => Assert.False(_validator.Validate(
            new CreateWarehouseCommand(code, "Main", null, null)).IsValid);

    [Fact]
    public void A_blank_name_is_invalid()
        => Assert.False(_validator.Validate(
            new CreateWarehouseCommand("WH-1", "", null, null)).IsValid);
}
