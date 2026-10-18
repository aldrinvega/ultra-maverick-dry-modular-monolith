using MediatR;
using Ultramaverick.Reporting.Application.Abstractions;
using Ultramaverick.Reporting.Domain.Entities;
using Ultramaverick.SharedKernel.Outbox;

namespace Ultramaverick.Reporting.Application.EventHandlers;

public sealed record QcReceiptCompletedNotification(
    int QcReceiptId,
    int ItemId,
    int UomId,
    string? LotNo,
    decimal NetQuantity,
    DateTime OccurredAtUtc,
    Guid EventId) : INotification;

/// <summary>Projects Quality's QcReceiptCompleted into Reporting.QcStatus.</summary>
public sealed class QcReceiptCompletedHandler : INotificationHandler<QcReceiptCompletedNotification>
{
    private const string Projection = nameof(QcReceiptCompletedHandler);

    private readonly IReportingUnitOfWork _uow;
    private readonly IProcessedEventStore _processedEvents;

    public QcReceiptCompletedHandler(IReportingUnitOfWork uow, IProcessedEventStore processedEvents)
    {
        _uow = uow;
        _processedEvents = processedEvents;
    }

    public async Task Handle(QcReceiptCompletedNotification notification, CancellationToken ct)
    {
        if (await _processedEvents.HasProcessedAsync(notification.EventId, Projection, ct))
            return;

        var status = await _uow.QcStatus.FindAsync(notification.QcReceiptId, ct);

        if (status is null)
        {
            await _uow.QcStatus.AddAsync(
                QcStatus.Create(
                    notification.QcReceiptId,
                    notification.ItemId,
                    notification.UomId,
                    notification.LotNo,
                    notification.NetQuantity,
                    notification.OccurredAtUtc),
                ct);
        }
        else
        {
            status.Update(notification.NetQuantity, notification.OccurredAtUtc);
        }

        await _processedEvents.MarkProcessedAsync(notification.EventId, Projection, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
