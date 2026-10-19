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
