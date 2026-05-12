using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WarehouseCV.Migrations
{
    /// <inheritdoc />
    public partial class AddedInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InventoryId",
                table: "Reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_InventoryId",
                table: "Reports",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_UserId",
                table: "Inventories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Inventories_InventoryId",
                table: "Reports",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Inventories_InventoryId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Reports_InventoryId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "Reports");
        }
    }
}
