using MediatR;
using Ultramaverick.Orders.Application.Models;
using Ultramaverick.Orders.Domain.Entities;

namespace Ultramaverick.Orders.Application.Commands.Orders;

public sealed record CreateOrderCommand(
    int CustomerId,
    DateTime? OrderDate,
    DateTime? DeliveryDate,
    string? Remarks,
    IReadOnlyList<OrderLineInput> Lines) : IRequest<Result<int>>;
