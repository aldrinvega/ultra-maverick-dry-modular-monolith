namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabTestProductCondition
{
    public int LabTestRequestId { get; private set; }
    public int ProductConditionId { get; private set; }

    private LabTestProductCondition() { }

    internal static LabTestProductCondition Create(int labTestRequestId, int productConditionId)
        => new() { LabTestRequestId = labTestRequestId, ProductConditionId = productConditionId };
}
