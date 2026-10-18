namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabTestSampleType
{
    public int LabTestRequestId { get; private set; }
    public int SampleTypeId { get; private set; }

    private LabTestSampleType() { }

    internal static LabTestSampleType Create(int labTestRequestId, int sampleTypeId)
        => new() { LabTestRequestId = labTestRequestId, SampleTypeId = sampleTypeId };
}
