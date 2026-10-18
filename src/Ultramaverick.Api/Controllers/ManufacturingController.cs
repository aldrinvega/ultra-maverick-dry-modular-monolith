using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;

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

        [HttpPost("AddTransformationRequest")]
        public async Task<IActionResult> AddTransformationRequest(
            [FromBody] CreateTransformationRequestCommand cmd, CancellationToken ct)
            => (await _mediator.Send(cmd, ct)).ToOk();

        [HttpPut("Prepare/{id}")]
        public async Task<IActionResult> Prepare(int id, CancellationToken ct)
            => (await _mediator.Send(new PrepareTransformationRequestCommand(id), ct)).ToNoContent();
    }
}
