using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Laboratory.Application.Commands.LabReceiveRequests;
using Ultramaverick.Laboratory.Application.Commands.LabTestRequests;
using Ultramaverick.Laboratory.Application.Commands.LabTestResults;
using Ultramaverick.Laboratory.Application.Models;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Laboratory-test endpoints. The api/LaboratoryTest prefix mirrors the legacy API surface.
    /// </summary>
    [ApiController]
    [Route("api/LaboratoryTest")]
    [Authorize(Policy = Policies.Laboratory)]
    public sealed class LabTestsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LabTestsController(IMediator mediator) => _mediator = mediator;

        [HttpPost("AddLabTestRequest")]
        public async Task<IActionResult> AddLabTestRequest(
            [FromBody] CreateLabTestRequestCommand cmd, CancellationToken ct)
            => (await _mediator.Send(cmd, ct)).ToOk();

        [HttpPost("AcceptRequest")]
        public async Task<IActionResult> AcceptRequest(
            [FromBody] AcceptLabReceiveRequestCommand cmd, CancellationToken ct)
            => (await _mediator.Send(cmd, ct)).ToOk();

        [HttpPut("RecordResult/{id:int}")]
        public async Task<IActionResult> RecordResult(
            int id, [FromBody] RecordLabResultCommand cmd, CancellationToken ct)
        {
            if (id != cmd.LabReceiveRequestId)
                return BadRequest(new ApiResponse(400, "Route id does not match payload id."));

            return (await _mediator.Send(cmd, ct)).ToNoContent();
        }
    }

    /// <summary>
    /// Maps Laboratory application results to HTTP responses. Laboratory owns its Result type,
    /// so the API adapts it here rather than reusing another module's mapping.
    /// </summary>
    internal static class LaboratoryResultExtensions
    {
        public static IActionResult ToOk(this Result<int> result)
            => result.Succeeded
                ? new OkObjectResult(result.Value)
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));

        public static IActionResult ToNoContent(this Result result)
            => result.Succeeded
                ? new NoContentResult()
                : new BadRequestObjectResult(new ApiResponse(400, result.Error));
    }
}
