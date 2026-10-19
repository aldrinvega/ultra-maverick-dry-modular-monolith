using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ultramaverick.Api.Authorization;
using Ultramaverick.Api.Errors;
using Ultramaverick.Orders.Application.Commands.Orders;
using Ultramaverick.Orders.Application.Queries.CancelledOrders;
using Ultramaverick.Orders.Application.Queries.Orders;

namespace Ultramaverick.Api.Controllers
{
    /// <summary>
    /// Orders endpoints. They live under api/Ordering because that is where the legacy API
    /// exposed POST /Ordering/AddOrder; the route prefix is preserved deliberately.
    /// </summary>
    [ApiController]
    [Route("api/Ordering")]
    [Authorize(Policy = Policies.Orders)]
    public sealed class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator) => _mediator = mediator;

        [HttpPost("AddOrder")]
        public async Task<IActionResult> AddOrder(
            [FromBody] CreateOrderCommand cmd, CancellationToken ct)
            => (await _mediator.Send(cmd, ct)).ToOk();

        [HttpPut("CancelOrder/{id}")]
        public async Task<IActionResult> CancelOrder(
            int id, [FromBody] CancelOrderCommand cmd, CancellationToken ct)
            => id != cmd.OrderId
                ? ResultExtensions.BadRequestError("Route id and body id must match.")
                : (await _mediator.Send(cmd, ct)).ToNoContent();

        [HttpGet("GetCancelledOrdersById/{customerId}")]
        public async Task<IActionResult> GetCancelledOrdersById(int customerId, CancellationToken ct)
            => (await _mediator.Send(new GetCancelledOrdersByCustomerQuery(customerId), ct)).ToOk();

        [HttpGet("GetAllListOfCancelledOrdersPagination")]
        public async Task<IActionResult> GetCancelledOrdersPagination(
            [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
            => (await _mediator.Send(new GetCancelledOrdersPagedQuery(page, pageSize), ct)).ToOk();

        [HttpGet("GetOrders")]
        public async Task<IActionResult> GetOrders(
            [FromQuery] int? customerId,
            [FromQuery] string? status,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            CancellationToken ct)
            => (await _mediator.Send(new GetOrdersQuery(customerId, status, page, pageSize), ct)).ToOk();

        [HttpGet("GetOrderById/{id}")]
        public async Task<IActionResult> GetOrderById(int id, CancellationToken ct)
            => (await _mediator.Send(new GetOrderByIdQuery(id), ct)).ToOk();

        [HttpGet("GetMoveOrders")]
        public async Task<IActionResult> GetMoveOrders(
            [FromQuery] int page, [FromQuery] int pageSize,
            [FromQuery] string? status, CancellationToken ct)
            => (await _mediator.Send(new GetMoveOrdersQuery(page, pageSize, status), ct)).ToOk();

        [HttpGet("GetTransactedMoveOrders")]
        public async Task<IActionResult> GetTransactedMoveOrders(
            [FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
            => (await _mediator.Send(new GetTransactedMoveOrdersQuery(page, pageSize), ct)).ToOk();
    }
}
