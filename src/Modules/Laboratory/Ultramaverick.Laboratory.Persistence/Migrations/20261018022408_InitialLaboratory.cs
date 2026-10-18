using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ultramaverick.Laboratory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLaboratory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Laboratory");

            migrationBuilder.CreateTable(
                name: "LabTestRequests",
                schema: "Laboratory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    WarehouseReceiptId = table.Column<int>(type: "int", nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TestType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateRequested = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateNeeded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "Laboratory",
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
                schema: "Laboratory",
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
                name: "LabReceiveRequests",
                schema: "Laboratory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    LabTestRequestId = table.Column<int>(type: "int", nullable: false),
                    DispositionId = table.Column<int>(type: "int", nullable: true),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedByUserId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabReceiveRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabReceiveRequests_LabTestRequests_LabTestRequestId",
                        column: x => x.LabTestRequestId,
                        principalSchema: "Laboratory",
                        principalTable: "LabTestRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabTestAnalyses",
                schema: "Laboratory",
                columns: table => new
                {
                    LabTestRequestId = table.Column<int>(type: "int", nullable: false),
                    AnalysisId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestAnalyses", x => new { x.LabTestRequestId, x.AnalysisId });
                    table.ForeignKey(
                        name: "FK_LabTestAnalyses_LabTestRequests_LabTestRequestId",
                        column: x => x.LabTestRequestId,
                        principalSchema: "Laboratory",
                        principalTable: "LabTestRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabTestParameters",
                schema: "Laboratory",
                columns: table => new
                {
                    LabTestRequestId = table.Column<int>(type: "int", nullable: false),
                    ParameterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestParameters", x => new { x.LabTestRequestId, x.ParameterId });
                    table.ForeignKey(
                        name: "FK_LabTestParameters_LabTestRequests_LabTestRequestId",
                        column: x => x.LabTestRequestId,
                        principalSchema: "Laboratory",
                        principalTable: "LabTestRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabTestProductConditions",
                schema: "Laboratory",
                columns: table => new
                {
                    LabTestRequestId = table.Column<int>(type: "int", nullable: false),
                    ProductConditionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestProductConditions", x => new { x.LabTestRequestId, x.ProductConditionId });
                    table.ForeignKey(
                        name: "FK_LabTestProductConditions_LabTestRequests_LabTestRequestId",
                        column: x => x.LabTestRequestId,
                        principalSchema: "Laboratory",
                        principalTable: "LabTestRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabTestSampleTypes",
                schema: "Laboratory",
                columns: table => new
                {
                    LabTestRequestId = table.Column<int>(type: "int", nullable: false),
                    SampleTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestSampleTypes", x => new { x.LabTestRequestId, x.SampleTypeId });
                    table.ForeignKey(
                        name: "FK_LabTestSampleTypes_LabTestRequests_LabTestRequestId",
                        column: x => x.LabTestRequestId,
                        principalSchema: "Laboratory",
                        principalTable: "LabTestRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabTestSwabTypes",
                schema: "Laboratory",
                columns: table => new
                {
                    LabTestRequestId = table.Column<int>(type: "int", nullable: false),
                    SwabTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestSwabTypes", x => new { x.LabTestRequestId, x.SwabTypeId });
                    table.ForeignKey(
                        name: "FK_LabTestSwabTypes_LabTestRequests_LabTestRequestId",
                        column: x => x.LabTestRequestId,
                        principalSchema: "Laboratory",
                        principalTable: "LabTestRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabRejectedItems",
                schema: "Laboratory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    LabReceiveRequestId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ReasonId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RejectedByUserId = table.Column<int>(type: "int", nullable: true),
                    RejectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabRejectedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabRejectedItems_LabReceiveRequests_LabReceiveRequestId",
                        column: x => x.LabReceiveRequestId,
                        principalSchema: "Laboratory",
                        principalTable: "LabReceiveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabReturnedItems",
                schema: "Laboratory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    LabReceiveRequestId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ReasonId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReturnedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReturnedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabReturnedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabReturnedItems_LabReceiveRequests_LabReceiveRequestId",
                        column: x => x.LabReceiveRequestId,
                        principalSchema: "Laboratory",
                        principalTable: "LabReceiveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabTestResults",
                schema: "Laboratory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    LabReceiveRequestId = table.Column<int>(type: "int", nullable: false),
                    DateAnalyzed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AnalyzedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabTestResults_LabReceiveRequests_LabReceiveRequestId",
                        column: x => x.LabReceiveRequestId,
                        principalSchema: "Laboratory",
                        principalTable: "LabReceiveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabReceiveRequests_LabTestRequestId",
                schema: "Laboratory",
                table: "LabReceiveRequests",
                column: "LabTestRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LabRejectedItems_LabReceiveRequestId",
                schema: "Laboratory",
                table: "LabRejectedItems",
                column: "LabReceiveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LabReturnedItems_LabReceiveRequestId",
                schema: "Laboratory",
                table: "LabReturnedItems",
                column: "LabReceiveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTestResults_LabReceiveRequestId",
                schema: "Laboratory",
                table: "LabTestResults",
                column: "LabReceiveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "Laboratory",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" },
                filter: "[PublishedAtUtc] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabRejectedItems",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "LabReturnedItems",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "LabTestAnalyses",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "LabTestParameters",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "LabTestProductConditions",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "LabTestResults",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "LabTestSampleTypes",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "LabTestSwabTypes",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "ProcessedEvents",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "LabReceiveRequests",
                schema: "Laboratory");

            migrationBuilder.DropTable(
                name: "LabTestRequests",
                schema: "Laboratory");
        }
    }
}
