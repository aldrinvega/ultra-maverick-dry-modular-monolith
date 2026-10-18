namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabTestSwabType
{
    public int LabTestRequestId { get; private set; }
    public int SwabTypeId { get; private set; }

    private LabTestSwabType() { }

    internal static LabTestSwabType Create(int labTestRequestId, int swabTypeId)
        => new() { LabTestRequestId = labTestRequestId, SwabTypeId = swabTypeId };
}
