using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ultramaverick.Api.Errors;
using Ultramaverick.Identity.Application.Commands.Authentication;
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
        [EnableRateLimiting("login")]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(
            [FromBody] AuthenticateRequest request, CancellationToken ct)
            => (await _mediator.Send(new AuthenticateCommand(request.UserName, request.Password), ct)).ToOk();

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshRequest request, CancellationToken ct)
            => (await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct)).ToOkOrUnauthorized();

        [AllowAnonymous]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(
            [FromBody] RefreshRequest request, CancellationToken ct)
            => (await _mediator.Send(new RevokeTokenCommand(request.RefreshToken), ct)).ToNoContent();
    }
}
