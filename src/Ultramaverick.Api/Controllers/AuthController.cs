using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Identity.Application.Commands.Authenticate;
using Ultramaverick.Identity.Application.Commands.RefreshToken;
using Ultramaverick.Identity.Application.Commands.RevokeToken;
using Ultramaverick.Identity.Application.Models;

namespace Ultramaverick.Api.Controllers
{
    [ApiController]
    [Route("api/Login")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(
            [FromBody] AuthenticateRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(
                new AuthenticateCommand(request.UserName, request.Password), ct);

            return result.Succeeded ? Ok(result.Value) : BadRequest(new { message = result.Error });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);

            return result.Succeeded ? Ok(result.Value) : Unauthorized(new { message = result.Error });
        }

        [AllowAnonymous]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(
            [FromBody] RefreshRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RevokeTokenCommand(request.RefreshToken), ct);

            return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
        }
    }
}
