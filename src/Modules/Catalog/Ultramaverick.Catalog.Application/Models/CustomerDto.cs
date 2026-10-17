namespace Ultramaverick.Catalog.Application.Models;

public sealed record CustomerDto(
    int Id,
    string CustomerCode,
    string CustomerName,
    int? FarmId,
    string? CompanyCode,
    string? CompanyName,
    int? AccountTitleId,
    int? LocationId,
    int? DepartmentId,
    string? MobileNumber,
    string? LeadMan,
    string? Address,
    bool IsActive);