using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Customers;

public sealed record SetCustomerActiveCommand(int CustomerId, bool IsActive) : IRequest<Result>;