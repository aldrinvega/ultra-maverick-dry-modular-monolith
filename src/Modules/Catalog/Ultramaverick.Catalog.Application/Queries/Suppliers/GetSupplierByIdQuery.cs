using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Queries.Suppliers;

public sealed record GetSupplierByIdQuery(int SupplierId) : IRequest<Result<SupplierDto>>;