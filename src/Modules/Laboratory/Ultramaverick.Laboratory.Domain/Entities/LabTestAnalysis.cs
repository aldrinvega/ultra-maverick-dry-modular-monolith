namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabTestAnalysis
{
    public int LabTestRequestId { get; private set; }
    public int AnalysisId { get; private set; }

    private LabTestAnalysis() { }

    internal static LabTestAnalysis Create(int labTestRequestId, int analysisId)
        => new() { LabTestRequestId = labTestRequestId, AnalysisId = analysisId };
}
