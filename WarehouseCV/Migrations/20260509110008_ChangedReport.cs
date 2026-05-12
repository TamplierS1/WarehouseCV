using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseCV.Migrations
{
    /// <inheritdoc />
    public partial class ChangedReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Detections_Reports_ReportId",
                table: "Detections");

            migrationBuilder.DropIndex(
                name: "IX_Detections_ReportId",
                table: "Detections");

            migrationBuilder.DropColumn(
                name: "Confirmed",
                table: "Reports");

            migrationBuilder.AddColumn<int>(
                name: "ObjectCount",
                table: "Reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ObjectName",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_UserId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_UserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ObjectCount",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ObjectName",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Reports");

            migrationBuilder.AddColumn<bool>(
                name: "Confirmed",
                table: "Reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Detections_ReportId",
                table: "Detections",
                column: "ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Detections_Reports_ReportId",
                table: "Detections",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
