using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCartUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId_ProductId",
                table: "Carts");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 50, 16, 748, DateTimeKind.Local).AddTicks(5184));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 50, 16, 748, DateTimeKind.Local).AddTicks(5186));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 50, 16, 748, DateTimeKind.Local).AddTicks(5188));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 50, 16, 748, DateTimeKind.Local).AddTicks(5190));

            migrationBuilder.UpdateData(
                table: "Sliders",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 50, 16, 748, DateTimeKind.Local).AddTicks(5472));

            migrationBuilder.UpdateData(
                table: "Sliders",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 50, 16, 748, DateTimeKind.Local).AddTicks(5476));

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId_ProductId_ProductSizeId",
                table: "Carts",
                columns: new[] { "UserId", "ProductId", "ProductSizeId" },
                unique: true,
                filter: "[ProductSizeId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId_ProductId_ProductSizeId",
                table: "Carts");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 32, 19, 703, DateTimeKind.Local).AddTicks(7442));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 32, 19, 703, DateTimeKind.Local).AddTicks(7445));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 32, 19, 703, DateTimeKind.Local).AddTicks(7447));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 32, 19, 703, DateTimeKind.Local).AddTicks(7449));

            migrationBuilder.UpdateData(
                table: "Sliders",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 32, 19, 703, DateTimeKind.Local).AddTicks(7642));

            migrationBuilder.UpdateData(
                table: "Sliders",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 12, 32, 19, 703, DateTimeKind.Local).AddTicks(7645));

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId_ProductId",
                table: "Carts",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }
    }
}
