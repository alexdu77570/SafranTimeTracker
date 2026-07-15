using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafranTimeTracker.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddLot2FinancialModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_contract_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    contract_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    contract_daily_rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    concurrency_stamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_contract_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_contract_histories_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resource_company_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    assignment_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_company_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_company_assignments_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_resource_company_assignments_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resource_tjm_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    daily_rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    concurrency_stamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_tjm_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_tjm_histories_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "id", "adresse", "code", "commentaire", "company_type_id", "contact_principal", "created_at", "created_by", "email_contact", "nom", "statut", "telephone", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0013-000000000002"), null, "EXTCONSEIL", "Société externe de démonstration (données de démonstration, Lot 2).", new Guid("00000000-0000-0000-0004-000000000002"), "Direction Commerciale", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "contact@externeconseil.local", "Externe Conseil", 0, null, null, null });

            migrationBuilder.InsertData(
                table: "resource_company_assignments",
                columns: new[] { "id", "assignment_type", "comment", "company_id", "created_at", "created_by", "end_date", "resource_id", "start_date", "status", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0032-000000000001"), "Principale", null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0021-000000000001"), new DateOnly(2024, 1, 1), 0, null, null },
                    { new Guid("00000000-0000-0000-0032-000000000003"), "Principale", null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0021-000000000003"), new DateOnly(2024, 1, 1), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "resource_tjm_histories",
                columns: new[] { "id", "comment", "concurrency_stamp", "created_at", "created_by", "daily_rate", "end_date", "reason", "resource_id", "start_date", "status", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0030-000000000001"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 650.00m, null, "TJM initial", new Guid("00000000-0000-0000-0021-000000000001"), new DateOnly(2024, 1, 1), 0, null, null },
                    { new Guid("00000000-0000-0000-0030-000000000002"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 700.00m, null, "TJM initial", new Guid("00000000-0000-0000-0021-000000000002"), new DateOnly(2024, 1, 1), 0, null, null },
                    { new Guid("00000000-0000-0000-0030-000000000003"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 600.00m, new DateOnly(2024, 12, 31), "TJM initial", new Guid("00000000-0000-0000-0021-000000000003"), new DateOnly(2024, 1, 1), 0, null, null },
                    { new Guid("00000000-0000-0000-0030-000000000004"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 620.00m, null, "Revalorisation annuelle", new Guid("00000000-0000-0000-0021-000000000003"), new DateOnly(2025, 1, 1), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "company_contract_histories",
                columns: new[] { "id", "comment", "company_id", "concurrency_stamp", "contract_daily_rate", "contract_number", "created_at", "created_by", "currency", "end_date", "start_date", "status", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0031-000000000001"), null, new Guid("00000000-0000-0000-0013-000000000002"), new Guid("00000000-0000-0000-0000-000000000000"), 750.00m, "CTR-2024-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "EUR", null, new DateOnly(2024, 1, 1), 0, null, null });

            migrationBuilder.InsertData(
                table: "resource_company_assignments",
                columns: new[] { "id", "assignment_type", "comment", "company_id", "created_at", "created_by", "end_date", "resource_id", "start_date", "status", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0032-000000000002"), "Principale", null, new Guid("00000000-0000-0000-0013-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0021-000000000002"), new DateOnly(2024, 1, 1), 0, null, null });

            migrationBuilder.CreateIndex(
                name: "ix_company_contract_histories_company_id_start_date",
                table: "company_contract_histories",
                columns: new[] { "company_id", "start_date" });

            migrationBuilder.CreateIndex(
                name: "ix_resource_company_assignments_company_id",
                table: "resource_company_assignments",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_company_assignments_resource_id_start_date",
                table: "resource_company_assignments",
                columns: new[] { "resource_id", "start_date" });

            migrationBuilder.CreateIndex(
                name: "ix_resource_tjm_histories_resource_id_start_date",
                table: "resource_tjm_histories",
                columns: new[] { "resource_id", "start_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "company_contract_histories");

            migrationBuilder.DropTable(
                name: "resource_company_assignments");

            migrationBuilder.DropTable(
                name: "resource_tjm_histories");

            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0013-000000000002"));
        }
    }
}
