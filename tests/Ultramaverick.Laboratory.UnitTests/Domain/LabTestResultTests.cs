using Ultramaverick.Laboratory.Contracts;
using Ultramaverick.Laboratory.Domain.Entities;

namespace Ultramaverick.Laboratory.UnitTests.Domain;

public class LabTestResultTests
{
    [Fact]
    public void Record_sets_for_qa_approval_and_raises_the_contract_event()
    {
        var analyzedAt = new DateTime(2026, 2, 3, 4, 5, 6, DateTimeKind.Utc);

        var result = LabTestResult.Record(5, 6, 7, "LOT-9", analyzedAt, "Passed", 8);

        Assert.Equal(5, result.LabReceiveRequestId);
        Assert.Equal(analyzedAt, result.DateAnalyzed);
        Assert.Equal("Passed", result.Result);
        Assert.Equal("ForQaApproval", result.Status);
        Assert.Equal(8, result.AnalyzedByUserId);
        Assert.True(result.IsActive);

        var raised = Assert.IsType<LabResultRecorded>(Assert.Single(result.DomainEvents));
        Assert.Equal(5, raised.LabReceiveRequestId);
        Assert.Equal(6, raised.LabTestRequestId);
        Assert.Equal(7, raised.WarehouseReceiptId);
        Assert.Equal("LOT-9", raised.BatchNo);
        Assert.Equal("Passed", raised.Result);
        Assert.Equal("ForQaApproval", raised.Status);
        Assert.Equal(8, raised.AnalyzedByUserId);
        Assert.Equal(analyzedAt, raised.DateAnalyzed);
    }

    [Fact]
    public void Approve_sets_approved_and_raises_the_distinct_approval_event()
    {
        var result = LabTestResult.Record(
            5, 6, 7, "LOT-9", DateTime.UtcNow, "Passed", 8);
        result.ClearDomainEvents();

        result.Approve(9);

        Assert.Equal("Approved", result.Status);

        var raised = Assert.IsType<LabResultApproved>(Assert.Single(result.DomainEvents));
        Assert.Equal(5, raised.LabReceiveRequestId);
        Assert.Equal("Approved", raised.Status);
        Assert.Equal(9, raised.ApprovedByUserId);
    }

    [Fact]
    public void Approve_when_already_approved_throws_and_does_not_raise_again()
    {
        var result = LabTestResult.Record(
            5, 6, 7, "LOT-9", DateTime.UtcNow, "Passed", 8);
        result.ClearDomainEvents();
        result.Approve(9);

        Assert.Throws<ArgumentOutOfRangeException>(() => result.Approve(10));
        Assert.Equal("Approved", result.Status);
        Assert.Single(result.DomainEvents);
    }
}
