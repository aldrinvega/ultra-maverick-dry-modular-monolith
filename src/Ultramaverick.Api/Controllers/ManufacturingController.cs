using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;
using Ultramaverick.Manufacturing.Application.Queries.TransformationRequests;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Manufacturing endpoints. They live under api/Manufacturing because that is where the
    /// legacy API exposed transformation requests; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/Manufacturing")]
    [Authorize(Policy = Policies.Manufacturing)]
    public sealed class ManufacturingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ManufacturingController(IMediator mediator) => _mediator = mediator;

        [HttpGet("GetTransformationRequests")]
        public async Task<IActionResult> GetTransformationRequests(
            [FromQuery] string? status, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTransformationRequestsQuery(status, page, pageSize), ct);
            return result.Succeeded ? Ok(result.Value) : BadRequest(new ApiResponse(400, result.Error));
        }

        [HttpGet("GetTransformationRequestById/{id}")]
        public async Task<IActionResult> GetTransformationRequestById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTransformationRequestByIdQuery(id), ct);
            return result.Succeeded ? Ok(result.Value) : BadRequest(new ApiResponse(400, result.Error));
        }
        [HttpPost("AddTransformationRequest")]
        public async Task<IActionResult> AddTransformationRequest(
            [FromBody] CreateTransformationRequestCommand cmd, CancellationToken ct)
            => (await _mediator.Send(cmd, ct)).ToOk();

        [HttpPut("Prepare/{id}")]
        public async Task<IActionResult> Prepare(int id, CancellationToken ct)
            => (await _mediator.Send(new PrepareTransformationRequestCommand(id), ct)).ToNoContent();
    }
}

