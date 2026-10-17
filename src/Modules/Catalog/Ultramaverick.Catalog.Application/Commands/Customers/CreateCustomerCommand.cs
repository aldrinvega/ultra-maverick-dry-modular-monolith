using MediatR;
using Ultramaverick.Catalog.Application.Models;

namespace Ultramaverick.Catalog.Application.Commands.Customers;

public sealed record CreateCustomerCommand(
    string CustomerCode, string CustomerName, int? FarmId, string? CompanyCode, string? CompanyName,
    int? AccountTitleId, int? LocationId, int? DepartmentId,
    string? MobileNumber, string? LeadMan, string? Address) : IRequest<Result<int>>;