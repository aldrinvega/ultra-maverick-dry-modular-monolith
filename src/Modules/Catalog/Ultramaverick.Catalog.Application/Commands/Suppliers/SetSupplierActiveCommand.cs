using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Suppliers;

public sealed record SetSupplierActiveCommand(int SupplierId, bool IsActive) : IRequest<Result>;