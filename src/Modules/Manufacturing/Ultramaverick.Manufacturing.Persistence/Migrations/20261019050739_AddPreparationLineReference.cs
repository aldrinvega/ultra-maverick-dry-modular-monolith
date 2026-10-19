using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ultramaverick.Manufacturing.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreparationLineReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransformationRequestLineId",
                schema: "Manufacturing",
                table: "TransformationRequestPreparations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransformationRequestLineId",
                schema: "Manufacturing",
                table: "TransformationRequestPreparations");
        }
    }
}
