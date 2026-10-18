namespace Ultramaverick.Warehouse.Domain;

/// <summary>
/// Why an inventory movement was written. Persisted as a string for ETL readability.
/// </summary>
public enum MovementSource
{
    QcReceipt = 1,
    MiscReceipt = 2,
    MiscIssue = 3,
    Adjustment = 4
}
