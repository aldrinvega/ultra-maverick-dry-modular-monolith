using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ultramaverick.Quality.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialQuality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Quality");

            migrationBuilder.CreateTable(
                name: "ChecklistTypes",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProductTypeId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "Quality",
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
                schema: "Quality",
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
                name: "QcReceipts",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderLineId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    ProductTypeId = table.Column<int>(type: "int", nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ManufacturingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDelivered = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalReject = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsNearlyExpire = table.Column<bool>(type: "bit", nullable: false),
                    ExpiryApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ExpiryApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QcByUserId = table.Column<int>(type: "int", nullable: true),
                    MonitoredByUserId = table.Column<int>(type: "int", nullable: true),
                    IsWarehouseReceived = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CancelReasonId = table.Column<int>(type: "int", nullable: true),
                    CancelRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QcReceipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistQuestions",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    ChecklistTypeId = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AnswerType = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistQuestions_ChecklistTypes_ChecklistTypeId",
                        column: x => x.ChecklistTypeId,
                        principalSchema: "Quality",
                        principalTable: "ChecklistTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Checklists",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    QcReceiptId = table.Column<int>(type: "int", nullable: false),
                    ProductTypeId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Checklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Checklists_QcReceipts_QcReceiptId",
                        column: x => x.QcReceiptId,
                        principalSchema: "Quality",
                        principalTable: "QcReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QcRejects",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    QcReceiptId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReasonId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RejectedByUserId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QcRejects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QcRejects_QcReceipts_QcReceiptId",
                        column: x => x.QcReceiptId,
                        principalSchema: "Quality",
                        principalTable: "QcReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistAnswers",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    ChecklistId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistAnswers_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalSchema: "Quality",
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistCompliances",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    ChecklistId = table.Column<int>(type: "int", nullable: false),
                    Compliance = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RootCause = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistCompliances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistCompliances_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalSchema: "Quality",
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistOpenFieldAnswers",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    ChecklistId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistOpenFieldAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistOpenFieldAnswers_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalSchema: "Quality",
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistOtherObservations",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    ChecklistId = table.Column<int>(type: "int", nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistOtherObservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistOtherObservations_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalSchema: "Quality",
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistProductDimensions",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    ChecklistId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    StandardValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ActualValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistProductDimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistProductDimensions_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalSchema: "Quality",
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistReviewVerificationLogs",
                schema: "Quality",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    ChecklistId = table.Column<int>(type: "int", nullable: false),
                    Disposition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistReviewVerificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistReviewVerificationLogs_Checklists_ChecklistId",
                        column: x => x.ChecklistId,
                        principalSchema: "Quality",
                        principalTable: "Checklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistAnswers_ChecklistId_QuestionId",
                schema: "Quality",
                table: "ChecklistAnswers",
                columns: new[] { "ChecklistId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistCompliances_ChecklistId",
                schema: "Quality",
                table: "ChecklistCompliances",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistOpenFieldAnswers_ChecklistId",
                schema: "Quality",
                table: "ChecklistOpenFieldAnswers",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistOtherObservations_ChecklistId",
                schema: "Quality",
                table: "ChecklistOtherObservations",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistProductDimensions_ChecklistId",
                schema: "Quality",
                table: "ChecklistProductDimensions",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistQuestions_ChecklistTypeId",
                schema: "Quality",
                table: "ChecklistQuestions",
                column: "ChecklistTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistReviewVerificationLogs_ChecklistId",
                schema: "Quality",
                table: "ChecklistReviewVerificationLogs",
                column: "ChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_Checklists_QcReceiptId",
                schema: "Quality",
                table: "Checklists",
                column: "QcReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "Quality",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" },
                filter: "[PublishedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QcReceipts_PurchaseOrderId",
                schema: "Quality",
                table: "QcReceipts",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_QcRejects_QcReceiptId",
                schema: "Quality",
                table: "QcRejects",
                column: "QcReceiptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistAnswers",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "ChecklistCompliances",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "ChecklistOpenFieldAnswers",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "ChecklistOtherObservations",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "ChecklistProductDimensions",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "ChecklistQuestions",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "ChecklistReviewVerificationLogs",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "ProcessedEvents",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "QcRejects",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "ChecklistTypes",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "Checklists",
                schema: "Quality");

            migrationBuilder.DropTable(
                name: "QcReceipts",
                schema: "Quality");
        }
    }
}
