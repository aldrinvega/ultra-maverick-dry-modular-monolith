namespace Ultramaverick.Manufacturing.Domain.Entities;

/// <summary>One required raw material on a transformation request. Carries no parent id: the
/// aggregate wires the foreign key from its navigation on save.</summary>
public sealed record TransformationLineInput(int ItemId, decimal RequiredQuantity);
