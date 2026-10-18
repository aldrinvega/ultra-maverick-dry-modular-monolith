using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ultramaverick.Warehouse.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Warehouse");

            migrationBuilder.CreateTable(
                name: "InventoryBalances",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OnHand = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Delta = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MiscellaneousIssues",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    RefNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IssuedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiscellaneousIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MiscellaneousReceipts",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    RefNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReceivedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiscellaneousReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "Warehouse",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AggregateId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedEvents",
                schema: "Warehouse",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectionName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedEvents", x => new { x.EventId, x.ProjectionName });
                });

            migrationBuilder.CreateTable(
                name: "StockReservations",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReservedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReservedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockReservations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseReceipts",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    QcReceiptId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ManufacturingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WarehouseTypeId = table.Column<int>(type: "int", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MiscellaneousIssueLines",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    MiscellaneousIssueId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiscellaneousIssueLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MiscellaneousIssueLines_MiscellaneousIssues_MiscellaneousIssueId",
                        column: x => x.MiscellaneousIssueId,
                        principalSchema: "Warehouse",
                        principalTable: "MiscellaneousIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MiscellaneousReceiptLines",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    MiscellaneousReceiptId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    LotNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MiscellaneousReceiptLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MiscellaneousReceiptLines_MiscellaneousReceipts_MiscellaneousReceiptId",
                        column: x => x.MiscellaneousReceiptId,
                        principalSchema: "Warehouse",
                        principalTable: "MiscellaneousReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryBalances_WarehouseId_ItemId_UomId_LotNo",
                schema: "Warehouse",
                table: "InventoryBalances",
                columns: new[] { "WarehouseId", "ItemId", "UomId", "LotNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_WarehouseId_ItemId",
                schema: "Warehouse",
                table: "InventoryMovements",
                columns: new[] { "WarehouseId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousIssueLines_MiscellaneousIssueId",
                schema: "Warehouse",
                table: "MiscellaneousIssueLines",
                column: "MiscellaneousIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_MiscellaneousReceiptLines_MiscellaneousReceiptId",
                schema: "Warehouse",
                table: "MiscellaneousReceiptLines",
                column: "MiscellaneousReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "Warehouse",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" },
                filter: "[PublishedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_OrderId_ItemId_WarehouseId",
                schema: "Warehouse",
                table: "StockReservations",
                columns: new[] { "OrderId", "ItemId", "WarehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_QcReceiptId",
                schema: "Warehouse",
                table: "WarehouseReceipts",
                column: "QcReceiptId",
                unique: true,
                filter: "[QcReceiptId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseReceipts_WarehouseId",
                schema: "Warehouse",
                table: "WarehouseReceipts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                schema: "Warehouse",
                table: "Warehouses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_IsActive",
                schema: "Warehouse",
                table: "Warehouses",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryBalances",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "InventoryMovements",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "MiscellaneousIssueLines",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "MiscellaneousReceiptLines",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "ProcessedEvents",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "StockReservations",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "WarehouseReceipts",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "Warehouses",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "MiscellaneousIssues",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "MiscellaneousReceipts",
                schema: "Warehouse");
        }
    }
}
