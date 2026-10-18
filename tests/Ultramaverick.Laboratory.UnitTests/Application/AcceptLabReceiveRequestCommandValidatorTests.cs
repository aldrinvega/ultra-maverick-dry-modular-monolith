using FluentValidation.TestHelper;
using Ultramaverick.Laboratory.Application.Commands.LabReceiveRequests;

namespace Ultramaverick.Laboratory.UnitTests.Application;

public class AcceptLabReceiveRequestCommandValidatorTests
{
    private readonly AcceptLabReceiveRequestCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.TestValidate(
            new AcceptLabReceiveRequestCommand(1, null, null, "ok"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_test_request_id_fails_on_LabTestRequestId(int id)
    {
        var result = _validator.TestValidate(
            new AcceptLabReceiveRequestCommand(id, null, null, null));

        Assert.False(result.IsValid);
        Assert.Equal("LabTestRequestId", Assert.Single(result.Errors).PropertyName);
    }
}
