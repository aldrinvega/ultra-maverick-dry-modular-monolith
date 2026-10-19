# Manufacturing finish / mixing — design decision

Finishing a transformation request turns prepared materials into a warehouse receipt. That write belongs
to the Warehouse module, so this is the one Manufacturing flow that crosses a module boundary.

## Decision: publish an integration event, do not write across schemas

Manufacturing owns the decision to finish; Warehouse owns receipt creation. The two are connected the way
every other cross-module flow in this solution already works — a `*.Contracts` event plus a consumer
bridge — not by Manufacturing writing into `Warehouse.*`.

Rejected alternatives:

- **Manufacturing writes `WarehouseReceipts` directly.** Violates the module boundary and the one legal
  cross-module surface (the contracts project). It would also bypass Warehouse's own invariants.
- **Manufacturing calls a Warehouse command in-process.** Creates a compile-time dependency between the
  two modules' application layers, which the architecture avoids everywhere else.

## State machine addition

`Prepared` → finish → `Finished`.

Guards:

- Only a `Prepared` request can be finished; anything else is a domain error.
- Finishing is idempotent: a second finish on a `Finished` request returns success without a second event.
- Cancel remains available only before finishing. Cancelling a finished request is a domain error.

## Contract

`TransformationCompleted(string ChangeType, DateTime OccurredAtUtc)` in
`Ultramaverick.Manufacturing.Contracts`, matching the id-free shape the other Manufacturing events use.
The payload deliberately carries no receipt data: the Warehouse consumer reads what it needs from the
request itself via the outbox aggregate id, or Manufacturing adds the fields to the contract if a lookup
proves necessary. **Decide this at implementation time by checking whether Warehouse can resolve the
request id — if not, the event must carry `ProductionPlanId`, `WarehouseId` and the prepared quantities.**

## Files to touch

| Project | Change |
|---|---|
| `Ultramaverick.Manufacturing.Domain` | `Finished` status, `Finish(...)` method raising `TransformationCompleted` |
| `Ultramaverick.Manufacturing.Contracts` | `TransformationCompleted`, `TransformationChangeTypes.Finished` |
| `Ultramaverick.Manufacturing.Application` | `FinishTransformationRequestCommand` + handler |
| `Ultramaverick.Api` | `PUT api/Manufacturing/Finish/{id}` |
| `Ultramaverick.Warehouse.Infrastructure` | `TransformationCompletedBridge` implementing `IIntegrationEventHandler`, registered in the module's DI |
| `Ultramaverick.Warehouse.Application` | handler that creates the receipt from the event, guarded by `IProcessedEventStore` for idempotency |

## Idempotency

The Warehouse handler must be guarded by `(EventId, projection)` exactly like the existing
`QcReceiptCompletedBridge` consumer, because the outbox retries on failure. Without the guard a redelivery
would create a duplicate receipt with no unique constraint to stop it.

## Testing

- Unit: `Prepared` → finish succeeds and raises one event; finishing twice raises one event; finishing a
  `Requested` request throws; cancelling a finished request throws.
- Integration: publish a `TransformationCompleted` outbox record and assert exactly one `WarehouseReceipts`
  row exists; republish the same `EventId` and assert the count is unchanged.

## Implementation finding (blocker)

Attempting the Warehouse consumer surfaced a modelling gap that the original design did not anticipate.

`WarehouseReceipt.Create` requires, per receipt: `warehouseId`, `itemId`, `uomId`, `quantity`, plus optional lot and dates.

What the domain actually records today:

- `TransformationRequestLine` holds `ItemId` and `RequiredQuantity` — the plan, not what was consumed.
- `TransformationRequestPreparation` holds a single `QuantityNeeded` per preparation, which the aggregate sets to the **sum of all line required quantities**.

So there is **no per-item consumed quantity** anywhere in the Manufacturing model. A finished request therefore cannot produce a correct receipt: we would have to guess which item each prepared quantity belongs to, or emit one receipt per line using planned rather than actual quantities. Both would write financially wrong stock.

### Consequence

The `TransformationCompleted` payload (ChangeType, ProductionPlanId, WarehouseId, OccurredAtUtc) is correct in shape but **insufficient in content**. Finishing currently raises the event, but no consumer can responsibly create a receipt from it.

### Required work before the consumer can exist

1. **Extend the preparation model** so a preparation records which line it belongs to and how much of that line was actually prepared — i.e. `TransformationRequestPreparation` gains `TransformationRequestLineId` and the aggregate stops collapsing quantities into one sum. That is a domain change plus a migration.
2. **Extend the contract** with `TransformationCompletedLine(int ItemId, int UomId, decimal PreparedQuantity)`, populated from those per-line preparations.
3. Only then add the Warehouse bridge and the guarded handler, one receipt per completed line.

Until step 1 lands, the finish endpoint must be treated as incomplete: it transitions state and emits an event, but it does not — and must not pretend to — create warehouse stock.
