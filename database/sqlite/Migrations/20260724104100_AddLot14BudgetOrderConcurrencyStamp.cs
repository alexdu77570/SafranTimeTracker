using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafranTimeTracker.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddLot14BudgetOrderConcurrencyStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "concurrency_stamp",
                table: "orders",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "concurrency_stamp",
                table: "budgets",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "budgets",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0061-000000000001"),
                column: "concurrency_stamp",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "budgets",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0095-000000000001"),
                column: "concurrency_stamp",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "budgets",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0095-000000000002"),
                column: "concurrency_stamp",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "budgets",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0095-000000000003"),
                column: "concurrency_stamp",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "orders",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0023-000000000001"),
                column: "concurrency_stamp",
                value: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "concurrency_stamp",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "concurrency_stamp",
                table: "budgets");
        }
    }
}
