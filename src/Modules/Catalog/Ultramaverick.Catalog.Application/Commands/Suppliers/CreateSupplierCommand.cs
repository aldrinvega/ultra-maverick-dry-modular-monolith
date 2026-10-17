using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Suppliers;

public sealed record CreateSupplierCommand(string SupplierCode, string SupplierName, string? SupplierAddress)
    : IRequest<Result<int>>;