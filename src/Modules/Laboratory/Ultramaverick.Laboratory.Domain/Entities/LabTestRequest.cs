using Ultramaverick.Laboratory.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Laboratory.Domain.Entities;

public sealed class LabTestRequest : Entity
{
    private readonly List<LabTestAnalysis> _analyses = new();
    private readonly List<LabTestParameter> _parameters = new();
    private readonly List<LabTestProductCondition> _productConditions = new();
    private readonly List<LabTestSampleType> _sampleTypes = new();
    private readonly List<LabTestSwabType> _swabTypes = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int? WarehouseReceiptId { get; private set; }   // scalar -> Warehouse
    public string? BatchNo { get; private set; }
    public decimal Quantity { get; private set; }
    public string? TestType { get; private set; }
    public DateTime? DateRequested { get; private set; }
    public DateTime? DateNeeded { get; private set; }
    public string Status { get; private set; } = LaboratoryStatuses.Requested;
    public string? Remarks { get; private set; }
    public int? RequestedByUserId { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<LabTestAnalysis> Analyses => _analyses.AsReadOnly();
    public IReadOnlyCollection<LabTestParameter> Parameters => _parameters.AsReadOnly();
    public IReadOnlyCollection<LabTestProductCondition> ProductConditions => _productConditions.AsReadOnly();
    public IReadOnlyCollection<LabTestSampleType> SampleTypes => _sampleTypes.AsReadOnly();
    public IReadOnlyCollection<LabTestSwabType> SwabTypes => _swabTypes.AsReadOnly();

    private LabTestRequest() { }

    public static LabTestRequest Create(
        int? warehouseReceiptId, string? batchNo, decimal quantity, string? testType,
        DateTime? dateRequested, DateTime? dateNeeded, int? requestedByUserId)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (warehouseReceiptId is <= 0) throw new ArgumentOutOfRangeException(nameof(warehouseReceiptId));

        var request = new LabTestRequest
        {
            WarehouseReceiptId = warehouseReceiptId,
            BatchNo = batchNo,
            Quantity = quantity,
            TestType = testType,
            DateRequested = dateRequested,
            DateNeeded = dateNeeded,
            RequestedByUserId = requestedByUserId,
            Status = LaboratoryStatuses.Requested,
            IsActive = true
        };

        request.RaiseDomainEvent(new LabTestRequestChanged(
            LabTestRequestChangeTypes.Created, DateTime.UtcNow));

        return request;
    }

    public void TagAnalyses(IEnumerable<int> ids)
    {
        foreach (var id in ids.Distinct())
            if (_analyses.All(x => x.AnalysisId != id))
                _analyses.Add(LabTestAnalysis.Create(Id, id));
    }

    public void TagParameters(IEnumerable<int> ids)
    {
        foreach (var id in ids.Distinct())
            if (_parameters.All(x => x.ParameterId != id))
                _parameters.Add(LabTestParameter.Create(Id, id));
    }

    public void TagProductConditions(IEnumerable<int> ids)
    {
        foreach (var id in ids.Distinct())
            if (_productConditions.All(x => x.ProductConditionId != id))
                _productConditions.Add(LabTestProductCondition.Create(Id, id));
    }

    public void TagSampleTypes(IEnumerable<int> ids)
    {
        foreach (var id in ids.Distinct())
            if (_sampleTypes.All(x => x.SampleTypeId != id))
                _sampleTypes.Add(LabTestSampleType.Create(Id, id));
    }

    public void TagSwabTypes(IEnumerable<int> ids)
    {
        foreach (var id in ids.Distinct())
            if (_swabTypes.All(x => x.SwabTypeId != id))
                _swabTypes.Add(LabTestSwabType.Create(Id, id));
    }
}
