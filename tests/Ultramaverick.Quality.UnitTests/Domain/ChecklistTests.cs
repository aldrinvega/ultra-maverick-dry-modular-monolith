using Ultramaverick.Quality.Domain.Entities;
using Ultramaverick.Quality.Domain.Events;

namespace Ultramaverick.Quality.UnitTests.Domain;

public class ChecklistTests
{
    [Fact]
    public void Child_accessors_accumulate_each_answer_kind()
    {
        var checklist = Checklist.For(1, null, 7);

        checklist.Answer(1, true);
        checklist.AnswerOpenField(2, "ok");
        checklist.RecordDimension(3, 1.5m, 1.6m);
        checklist.AddObservation("looks fine");
        checklist.AddCompliance("HACCP", "documented", null);

        Assert.Single(checklist.Answers);
        Assert.Single(checklist.OpenFields);
        Assert.Single(checklist.Dimensions);
        Assert.Single(checklist.Observations);
        Assert.Single(checklist.Compliances);
    }

    [Fact]
    public void Submit_raises_ChecklistSubmitted()
    {
        var checklist = Checklist.For(42, 5, 7);

        checklist.Submit();

        var raised = Assert.IsType<ChecklistSubmitted>(Assert.Single(checklist.DomainEvents));
        Assert.Equal(42, raised.QcReceiptId);
    }

    [Fact]
    public void For_captures_receipt_product_type_and_creator()
    {
        var checklist = Checklist.For(9, 3, 7);

        Assert.Equal(9, checklist.QcReceiptId);
        Assert.Equal(3, checklist.ProductTypeId);
        Assert.Equal(7, checklist.CreatedByUserId);
    }
}
