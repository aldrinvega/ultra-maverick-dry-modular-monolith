using Ultramaverick.Procurement.Application.Commands.PurchaseOrders;
using Ultramaverick.Procurement.Domain.Entities;
using Ultramaverick.Procurement.UnitTests.Fakes;

namespace Ultramaverick.Procurement.UnitTests.Application;

public class CreatePurchaseOrderCommandHandlerTests
{
    private static (CreatePurchaseOrderCommandHandler Handler, FakeProcurementUnitOfWork Uow,
        FakeSupplierLookup Suppliers) Build()
    {
        var uow = new FakeProcurementUnitOfWork();
        var suppliers = new FakeSupplierLookup();
        suppliers.ExistingSupplierIds.Add(1);
        var handler = new CreatePurchaseOrderCommandHandler(uow, suppliers, new FakeCurrentUser { UserId = 7 });
        return (handler, uow, suppliers);
    }

    private static CreatePurchaseOrderCommand Command(string poNumber = "PO-1", int supplierId = 1)
        => new(poNumber, null, null, null, supplierId,
            new[] { new PurchaseOrderLineInput(10, 20, 3m, 4.5m) });

    [Fact]
    public async Task Handle_creates_the_purchase_order_and_saves_once()
    {
        var (handler, uow, _) = Build();

        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, uow.SaveCount);
        Assert.Single(uow.PurchaseOrdersRepo.All);
        Assert.Equal("PO-1", uow.PurchaseOrdersRepo.All[0].PoNumber);
    }

    [Fact]
    public async Task Handle_rejects_a_duplicate_po_number()
    {
        var (handler, uow, _) = Build();
        uow.PurchaseOrdersRepo.Seed(PurchaseOrder.Create("PO-1", null, null, null, 1, null, null));

        var result = await handler.Handle(Command(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task Handle_trims_the_po_number_before_the_duplicate_check()
    {
        var (handler, uow, _) = Build();
        uow.PurchaseOrdersRepo.Seed(PurchaseOrder.Create("PO-1", null, null, null, 1, null, null));

        var result = await handler.Handle(Command(" PO-1 "), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task Handle_rejects_an_unknown_supplier()
    {
        var (handler, uow, _) = Build();

        var result = await handler.Handle(Command(supplierId: 99), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, uow.SaveCount);
    }

    [Fact]
    public async Task Handle_stores_the_normalized_po_number()
    {
        var (handler, uow, _) = Build();

        var result = await handler.Handle(Command(" PO-9 "), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("PO-9", uow.PurchaseOrdersRepo.All[0].PoNumber);
    }
}
