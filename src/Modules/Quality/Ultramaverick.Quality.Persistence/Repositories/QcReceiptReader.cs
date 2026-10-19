using Microsoft.EntityFrameworkCore;
using Ultramaverick.Quality.Application.Abstractions;
using Ultramaverick.Quality.Application.Queries.QcReceipts;

namespace Ultramaverick.Quality.Persistence.Repositories;

public sealed class QcReceiptReader : IQcReceiptReader
{
    private readonly QualityDbContext _context;

    public QcReceiptReader(QualityDbContext context) => _context = context;

    public async Task<QcReceiptDetailDto?> GetByIdAsync(int qcReceiptId, CancellationToken ct)
    {
        var receipt = await _context.QcReceipts
            .AsNoTracking()
            .Where(x => x.Id == qcReceiptId)
            .Select(x => new QcReceiptDetailDto(
                x.Id,
                x.PurchaseOrderId,
                x.PurchaseOrderLineId,
                x.ItemId,
                x.UomId,
                x.ProductTypeId,
                x.BatchNo,
                x.ManufacturingDate,
                x.ExpiryDate,
                x.ActualDelivered,
                x.TotalReject,
                x.IsNearlyExpire,
                x.IsWarehouseReceived,
                x.Status,
                x.IsActive,
                new List<QcChecklistAnswerDto>()))
            .FirstOrDefaultAsync(ct);

        if (receipt is null)
            return null;

        var answers = await (
                from checklist in _context.Checklists.AsNoTracking()
                where checklist.QcReceiptId == qcReceiptId
                join answer in _context.ChecklistAnswers.AsNoTracking()
                    on checklist.Id equals answer.ChecklistId
                join question in _context.ChecklistQuestions.AsNoTracking()
                    on answer.QuestionId equals question.Id
                select new QcChecklistAnswerDto(question.Question, answer.Status, null))
            .ToListAsync(ct);

        return receipt with { ChecklistAnswers = answers };
    }
}
