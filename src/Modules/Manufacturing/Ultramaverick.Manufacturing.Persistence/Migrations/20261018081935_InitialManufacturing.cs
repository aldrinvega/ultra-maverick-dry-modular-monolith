using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ultramaverick.Manufacturing.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialManufacturing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Manufacturing");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "Manufacturing",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AggregateId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                schema: "Manufacturing",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedEvents", x => new { x.EventId, x.ProjectionName });
                });

            migrationBuilder.CreateTable(
                name: "ProductionPlans",
                schema: "Manufacturing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    ProdPlanNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Version = table.Column<int>(type: "int", nullable: true),
                    Batch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPrepared = table.Column<bool>(type: "bit", nullable: false),
                    RejectedByUserId = table.Column<int>(type: "int", nullable: true),
                    RejectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancelRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsMixed = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransformationRequests",
                schema: "Manufacturing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    ProductionPlanId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransformationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransformationRequests_ProductionPlans_ProductionPlanId",
                        column: x => x.ProductionPlanId,
                        principalSchema: "Manufacturing",
                        principalTable: "ProductionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransformationRequestLines",
                schema: "Manufacturing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    TransformationRequestId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransformationRequestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransformationRequestLines_TransformationRequests_TransformationRequestId",
                        column: x => x.TransformationRequestId,
                        principalSchema: "Manufacturing",
                        principalTable: "TransformationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransformationRequestPreparations",
                schema: "Manufacturing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    TransformationRequestId = table.Column<int>(type: "int", nullable: false),
                    PreparedByUserId = table.Column<int>(type: "int", nullable: true),
                    PreparedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityNeeded = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    WeighingScale = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransformationRequestPreparations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransformationRequestPreparations_TransformationRequests_TransformationRequestId",
                        column: x => x.TransformationRequestId,
                        principalSchema: "Manufacturing",
                        principalTable: "TransformationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransformationRequestRejects",
                schema: "Manufacturing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LegacyId = table.Column<int>(type: "int", nullable: true),
                    TransformationRequestId = table.Column<int>(type: "int", nullable: false),
                    ReasonId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RejectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransformationRequestRejects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransformationRequestRejects_TransformationRequests_TransformationRequestId",
                        column: x => x.TransformationRequestId,
                        principalSchema: "Manufacturing",
                        principalTable: "TransformationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAtUtc_OccurredAtUtc",
                schema: "Manufacturing",
                table: "OutboxMessages",
                columns: new[] { "PublishedAtUtc", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPlans_ItemId",
                schema: "Manufacturing",
                table: "ProductionPlans",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TransformationRequestLines_TransformationRequestId",
                schema: "Manufacturing",
                table: "TransformationRequestLines",
                column: "TransformationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TransformationRequestPreparations_TransformationRequestId",
                schema: "Manufacturing",
                table: "TransformationRequestPreparations",
                column: "TransformationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TransformationRequestRejects_TransformationRequestId",
                schema: "Manufacturing",
                table: "TransformationRequestRejects",
                column: "TransformationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TransformationRequests_ProductionPlanId",
                schema: "Manufacturing",
                table: "TransformationRequests",
                column: "ProductionPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "Manufacturing");

            migrationBuilder.DropTable(
                name: "ProcessedEvents",
                schema: "Manufacturing");

            migrationBuilder.DropTable(
                name: "TransformationRequestLines",
                schema: "Manufacturing");

            migrationBuilder.DropTable(
                name: "TransformationRequestPreparations",
                schema: "Manufacturing");

            migrationBuilder.DropTable(
                name: "TransformationRequestRejects",
                schema: "Manufacturing");

            migrationBuilder.DropTable(
                name: "TransformationRequests",
                schema: "Manufacturing");

            migrationBuilder.DropTable(
                name: "ProductionPlans",
                schema: "Manufacturing");
        }
    }
}
