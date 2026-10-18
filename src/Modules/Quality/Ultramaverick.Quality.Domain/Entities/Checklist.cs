using Ultramaverick.Quality.Domain.Events;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Quality.Domain.Entities;

public sealed class Checklist : Entity
{
    private readonly List<ChecklistAnswer> _answers = new();
    private readonly List<ChecklistOpenFieldAnswer> _openFields = new();
    private readonly List<ChecklistProductDimension> _dimensions = new();
    private readonly List<ChecklistOtherObservation> _observations = new();
    private readonly List<ChecklistCompliance> _compliances = new();

    public int Id { get; private set; }
    public int? LegacyId { get; private set; }
    public int QcReceiptId { get; private set; }
    public int? ProductTypeId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public int? CreatedByUserId { get; private set; }

    public IReadOnlyCollection<ChecklistAnswer> Answers => _answers.AsReadOnly();
    public IReadOnlyCollection<ChecklistOpenFieldAnswer> OpenFields => _openFields.AsReadOnly();
    public IReadOnlyCollection<ChecklistProductDimension> Dimensions => _dimensions.AsReadOnly();
    public IReadOnlyCollection<ChecklistOtherObservation> Observations => _observations.AsReadOnly();
    public IReadOnlyCollection<ChecklistCompliance> Compliances => _compliances.AsReadOnly();

    private Checklist() { }

    public static Checklist For(int qcReceiptId, int? productTypeId, int? createdByUserId)
        => new() { QcReceiptId = qcReceiptId, ProductTypeId = productTypeId, CreatedByUserId = createdByUserId };

    public void Answer(int questionId, bool status) => _answers.Add(ChecklistAnswer.Create(Id, questionId, status));
    public void AnswerOpenField(int questionId, string remarks) => _openFields.Add(ChecklistOpenFieldAnswer.Create(Id, questionId, remarks));
    public void RecordDimension(int questionId, decimal? standard, decimal? actual)
        => _dimensions.Add(ChecklistProductDimension.Create(Id, questionId, standard, actual));
    public void AddObservation(string text) => _observations.Add(ChecklistOtherObservation.Create(Id, text));
    public void AddCompliance(string compliance, string? description, string? rootCause)
        => _compliances.Add(ChecklistCompliance.Create(Id, compliance, description, rootCause));

    public void Submit() => RaiseDomainEvent(new ChecklistSubmitted(Id, QcReceiptId, DateTime.UtcNow));
}
