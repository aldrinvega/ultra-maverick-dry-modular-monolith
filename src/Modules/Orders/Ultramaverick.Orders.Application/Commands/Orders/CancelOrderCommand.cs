using MediatR;
using Ultramaverick.Orders.Application.Models;

namespace Ultramaverick.Orders.Application.Commands.Orders;

public sealed record CancelOrderCommand(int OrderId, string? Reason) : IRequest<Result>;
