using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WarehouseCV.Migrations
{
    /// <inheritdoc />
    public partial class AddedProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Detections");

            migrationBuilder.DropColumn(
                name: "ObjectName",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "ObjectCount",
                table: "Reports",
                newName: "ProductId");

            migrationBuilder.AddColumn<int>(
                name: "ProductCount",
                table: "Reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Mark = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Designation = table.Column<string>(type: "text", nullable: false),
                    Classifier = table.Column<string>(type: "text", nullable: false),
                    ClassifierGroup = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ProductId",
                table: "Reports",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Products_ProductId",
                table: "Reports",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Products_ProductId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ProductId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ProductCount",
                table: "Reports");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Reports",
                newName: "ObjectCount");

            migrationBuilder.AddColumn<string>(
                name: "ObjectName",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Detections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BoundingBoxHeight = table.Column<int>(type: "integer", nullable: false),
                    BoundingBoxWidth = table.Column<int>(type: "integer", nullable: false),
                    BoundingBoxX = table.Column<int>(type: "integer", nullable: false),
                    BoundingBoxY = table.Column<int>(type: "integer", nullable: false),
                    ClassName = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    ReportId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detections", x => x.Id);
                });
        }
    }
}
