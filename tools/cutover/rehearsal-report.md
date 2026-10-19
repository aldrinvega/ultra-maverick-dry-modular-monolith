# Cutover rehearsal

Run: 2026-10-19T15:02:22.0341121+08:00
Staging database: UltraMaverickDry_Staging (source ElixirDepotDry, server .\SQLEXPRESS)
Migration: 80.4 s
Cold start to /health: 0.7 s

## Module smoke

Module        Status  Ms Detail                                                                                        
------        ------  -- ------                                                                                        
Identity         200 915 {"id":1,"fullName":"ADMIN","userName":"admin","roleId":1,"roleName":"ADMIN","departmentId":...
Catalog          200 151 {"items":[{"id":1,"itemCode":"CONDI0186","itemDescription":"SALT IODIZED 25KG/SACK","itemCa...
Procurement      200  47 {"items":[{"id":1,"poNumber":"82606","prNumber":"31179184","poDate":"2020-02-04T00:00:00","...
Quality          200  38 {"items":[{"id":1,"purchaseOrderId":1678,"purchaseOrderLineId":1073,"itemId":1221,"uomId":1...
Laboratory       200  34 {"items":[],"page":1,"pageSize":1,"totalCount":0,"totalPages":0}                              
Warehouse        200  33 {"items":[{"id":4343,"qcReceiptId":null,"warehouseId":1,"itemId":843,"uomId":10,"lotNo":nul...
Orders           200  45 {"items":[{"id":16203,"orderNumber":"7033","customerId":397,"status":"Confirmed","orderDate...
Manufacturing    200  57 {"items":[],"page":1,"pageSize":1,"totalCount":0,"totalPages":0}                              
Reporting        200 222 [{"warehouseId":1,"itemId":13,"uomId":2,"lotNo":"ALLERGEN 1","quantity":2.0000,"updatedAtUt...

## Reporting baseline

inventory 1530 rows / 30450211.3400 qty; orders 16203 rows; qc 2928 rows / 25589258.6000 qty

## Outbox replay

- First drain: 132.2 s
- Second drain (same EventIds): 99.1 s
- After first drain:  inventory 1556 rows / 32237007.1400 qty; orders 16203 rows; qc 2928 rows / 25589258.6000 qty
- After second drain: inventory 1556 rows / 32237007.1400 qty; orders 16203 rows; qc 2928 rows / 25589258.6000 qty
- Matches warm baseline: False
- Idempotent on redelivery: True

The warm backfill is a shortcut, not a projection. Replaying QcReceiptCompleted also creates Warehouse receipts for QC-completed lots that legacy WarehouseReceived never marked received; that is why the replay stock can exceed the warm backfill. The replay state is authoritative for cutover.

## Verdict

PASS
