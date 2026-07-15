using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafranTimeTracker.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddLot5BudgetsAndReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "budget_financier_ajuste",
                table: "orders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "budget_jours_ajuste",
                table: "orders",
                type: "decimal(9,2)",
                precision: 9,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "date_fin_ajustee",
                table: "orders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "budgets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    initial_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    adjusted_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    alert_threshold = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_budgets", x => x.id);
                    table.ForeignKey(
                        name: "fk_budgets_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_budgets_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dashboard_kpis",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    category = table.Column<int>(type: "int", nullable: false),
                    ordre = table.Column<int>(type: "int", nullable: false),
                    statut = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dashboard_kpis", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "export_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    generated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    generated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    app_version = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    report_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    format = table.Column<int>(type: "int", nullable: false),
                    filters_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contains_financial_data = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_export_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_extensions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    extension_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount_added = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    days_added = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    previous_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    new_end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_extensions", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_extensions_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "budget_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    budget_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    old_value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    new_value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    reference_piece = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_budget_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_budget_versions_budgets_budget_id",
                        column: x => x.budget_id,
                        principalTable: "budgets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "budgets",
                columns: new[] { "id", "adjusted_amount", "alert_threshold", "comment", "created_at", "created_by", "end_date", "initial_amount", "name", "order_id", "project_id", "start_date", "status", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0061-000000000001"), 180000.00m, 90m, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, 150000.00m, "Budget Migration ELM", null, new Guid("00000000-0000-0000-0052-000000000001"), new DateOnly(2024, 1, 1), 0, null, null });

            migrationBuilder.InsertData(
                table: "dashboard_kpis",
                columns: new[] { "id", "category", "code", "created_at", "created_by", "libelle", "ordre", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0063-000000000001"), 0, "TEMPS_SAISIS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Temps saisis sur la période", 1, 0, null, null },
                    { new Guid("00000000-0000-0000-0063-000000000002"), 0, "CAPACITE_REELLE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Capacité réelle", 2, 0, null, null },
                    { new Guid("00000000-0000-0000-0063-000000000003"), 0, "TAUX_DISPONIBILITE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Taux de disponibilité", 3, 0, null, null },
                    { new Guid("00000000-0000-0000-0063-000000000004"), 0, "CHARGE_RUN_HORS_RUN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Charge RUN / hors RUN", 4, 0, null, null },
                    { new Guid("00000000-0000-0000-0063-000000000005"), 0, "PROJETS_ACTIFS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Projets actifs", 5, 0, null, null },
                    { new Guid("00000000-0000-0000-0063-000000000006"), 0, "JALONS_EN_RETARD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Jalons en retard", 6, 0, null, null },
                    { new Guid("00000000-0000-0000-0063-000000000007"), 1, "BUDGET_RESTANT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Budget restant", 7, 0, null, null },
                    { new Guid("00000000-0000-0000-0063-000000000008"), 1, "DIFFERENTIEL_GLOBAL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Différentiel global", 8, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "order_extensions",
                columns: new[] { "id", "amount_added", "comment", "created_at", "created_by", "days_added", "extension_date", "new_end_date", "order_id", "previous_end_date", "reason" },
                values: new object[] { new Guid("00000000-0000-0000-0060-000000000001"), 20000.00m, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 30m, new DateOnly(2026, 6, 1), new DateOnly(2027, 3, 31), new Guid("00000000-0000-0000-0023-000000000001"), new DateOnly(2026, 12, 31), "Extension de périmètre validée par le comité de pilotage (démonstration)." });

            migrationBuilder.UpdateData(
                table: "orders",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0023-000000000001"),
                columns: new[] { "budget_financier_ajuste", "budget_jours_ajuste", "date_fin_ajustee", "project_id" },
                values: new object[] { 170000.00m, 230m, new DateOnly(2027, 3, 31), null });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "code", "description", "libelle" },
                values: new object[] { new Guid("00000000-0000-0000-0002-000000000002"), "TIME_ENTRY_CORRECTION", "Autorise une saisie de temps sur une commande clôturée, à titre de correction (cahier des charges §13.4).", "Correction de saisie sur commande clôturée" });

            migrationBuilder.InsertData(
                table: "budget_versions",
                columns: new[] { "id", "budget_id", "created_at", "created_by", "new_value", "old_value", "reason", "reference_piece" },
                values: new object[] { new Guid("00000000-0000-0000-0062-000000000001"), new Guid("00000000-0000-0000-0061-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 180000.00m, 150000.00m, "Dérive planning nécessitant un budget ajusté (démonstration, cf. Project.DateFinAjustee).", null });

            migrationBuilder.InsertData(
                table: "user_permissions",
                columns: new[] { "permission_id", "user_id", "granted_at", "granted_by" },
                values: new object[] { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0020-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_project_id",
                table: "orders",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_budget_versions_budget_id",
                table: "budget_versions",
                column: "budget_id");

            migrationBuilder.CreateIndex(
                name: "ix_budgets_order_id",
                table: "budgets",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_budgets_project_id",
                table: "budgets",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_dashboard_kpis_code",
                table: "dashboard_kpis",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_export_logs_generated_at",
                table: "export_logs",
                column: "generated_at");

            migrationBuilder.CreateIndex(
                name: "ix_order_extensions_order_id",
                table: "order_extensions",
                column: "order_id");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_projects_project_id",
                table: "orders",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_projects_project_id",
                table: "orders");

            migrationBuilder.DropTable(
                name: "budget_versions");

            migrationBuilder.DropTable(
                name: "dashboard_kpis");

            migrationBuilder.DropTable(
                name: "export_logs");

            migrationBuilder.DropTable(
                name: "order_extensions");

            migrationBuilder.DropTable(
                name: "budgets");

            migrationBuilder.DropIndex(
                name: "ix_orders_project_id",
                table: "orders");

            migrationBuilder.DeleteData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0020-000000000001") });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000002"));

            migrationBuilder.DropColumn(
                name: "budget_financier_ajuste",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "budget_jours_ajuste",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "date_fin_ajustee",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "orders");
        }
    }
}
