namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabTestParameter
{
    public int LabTestRequestId { get; private set; }
    public int ParameterId { get; private set; }

    private LabTestParameter() { }

    internal static LabTestParameter Create(int labTestRequestId, int parameterId)
        => new() { LabTestRequestId = labTestRequestId, ParameterId = parameterId };
}
