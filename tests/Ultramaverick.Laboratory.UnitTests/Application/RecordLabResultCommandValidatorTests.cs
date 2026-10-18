using FluentValidation.TestHelper;
using Ultramaverick.Laboratory.Application.Commands.LabTestResults;

namespace Ultramaverick.Laboratory.UnitTests.Application;

public class RecordLabResultCommandValidatorTests
{
    private readonly RecordLabResultCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.TestValidate(
            new RecordLabResultCommand(1, DateTime.UtcNow, "Passed"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_receive_request_id_fails_on_LabReceiveRequestId(int id)
    {
        var result = _validator.TestValidate(
            new RecordLabResultCommand(id, DateTime.UtcNow, "Passed"));

        Assert.False(result.IsValid);
        Assert.Equal("LabReceiveRequestId", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_result_fails_on_Result(string? value)
    {
        var result = _validator.TestValidate(
            new RecordLabResultCommand(1, DateTime.UtcNow, value!));

        Assert.False(result.IsValid);
        Assert.Equal("Result", Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Overlong_result_fails_on_Result()
    {
        var result = _validator.TestValidate(
            new RecordLabResultCommand(1, DateTime.UtcNow, new string('x', 2001)));

        Assert.False(result.IsValid);
        Assert.Equal("Result", Assert.Single(result.Errors).PropertyName);
    }
}
