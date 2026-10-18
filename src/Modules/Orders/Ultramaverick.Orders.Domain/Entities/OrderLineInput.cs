namespace Ultramaverick.Orders.Domain.Entities;

/// <summary>Input for one order line. Carried to <see cref="Order.Create"/>; the line's OrderId is
/// populated by EF on save, so the input never carries a parent id.</summary>
public sealed record OrderLineInput(int ItemId, int UomId, decimal Quantity, int? FormulaHeaderId);
