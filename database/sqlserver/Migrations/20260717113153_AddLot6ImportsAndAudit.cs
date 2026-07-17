using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafranTimeTracker.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddLot6ImportsAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    author = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    technical_context = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "import_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    import_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    mode = table.Column<int>(type: "int", nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    line_count = table.Column<int>(type: "int", nullable: false),
                    add_count = table.Column<int>(type: "int", nullable: false),
                    update_count = table.Column<int>(type: "int", nullable: false),
                    delete_count = table.Column<int>(type: "int", nullable: false),
                    error_count = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    errors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    checksum = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    previous_batch_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_receipts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    receipt_date = table.Column<DateOnly>(type: "date", nullable: false),
                    received_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    received_days = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_receipts", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_receipts_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "import_diffs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    import_batch_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    diff_type = table.Column<int>(type: "int", nullable: false),
                    field_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_import_diffs", x => x.id);
                    table.ForeignKey(
                        name: "fk_import_diffs_import_batches_import_batch_id",
                        column: x => x.import_batch_id,
                        principalTable: "import_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "activity_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0040-000000000012"),
                column: "is_run",
                value: true);

            migrationBuilder.UpdateData(
                table: "activity_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0040-000000000013"),
                column: "is_run",
                value: true);

            migrationBuilder.InsertData(
                table: "audit_logs",
                columns: new[] { "id", "action", "author", "entity_id", "entity_type", "new_value", "old_value", "reason", "technical_context", "timestamp" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0073-000000000001"), "EXTENSION", "system-seed", new Guid("00000000-0000-0000-0023-000000000001"), "Order", "{\"BudgetFinancierAjuste\":170000.00}", "{\"BudgetFinancierAjuste\":150000.00}", "Extension de périmètre validée par le comité de pilotage (démonstration, cf. OrderExtensionDemo).", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("00000000-0000-0000-0073-000000000002"), "IMPORT", "system-seed", new Guid("00000000-0000-0000-0071-000000000001"), "Applications", "{\"Mode\":\"MiseAJour\",\"AddCount\":1,\"UpdateCount\":1}", null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "import_batches",
                columns: new[] { "id", "add_count", "checksum", "delete_count", "error_count", "errors", "file_name", "import_date", "line_count", "mode", "previous_batch_id", "source", "status", "type", "update_count", "user_id" },
                values: new object[] { new Guid("00000000-0000-0000-0071-000000000001"), 1, "demo-checksum-lot6", 0, 0, null, "applications-demo.csv", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, null, "CSV", 2, 14, 1, "system-seed" });

            migrationBuilder.InsertData(
                table: "order_receipts",
                columns: new[] { "id", "comment", "created_at", "created_by", "order_id", "reason", "receipt_date", "received_amount", "received_days" },
                values: new object[] { new Guid("00000000-0000-0000-0070-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0023-000000000001"), "Première réception partielle (démonstration).", new DateOnly(2026, 3, 1), 15000.00m, null });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "code", "description", "libelle" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0002-000000000003"), "USER_ADMINISTRATION", "Autorise la modification/désactivation d'un utilisateur, le changement de rôle et l'octroi/retrait de permission (cahier des charges §28.3).", "Administration des utilisateurs" },
                    { new Guid("00000000-0000-0000-0002-000000000004"), "TIME_ENTRY_RECALCULATION", "Autorise le recalcul explicite d'une saisie déjà valorisée (cahier des charges §19.6).", "Recalcul financier explicite d'une saisie" },
                    { new Guid("00000000-0000-0000-0002-000000000005"), "IMPORT_EXECUTE", "Autorise l'aperçu, la simulation et l'exécution d'un import (cahier des charges §27).", "Exécution d'un import" },
                    { new Guid("00000000-0000-0000-0002-000000000006"), "AUDIT_VIEW", "Autorise la consultation du journal d'audit (cahier des charges §28.1).", "Consultation du journal d'audit" },
                    { new Guid("00000000-0000-0000-0002-000000000007"), "ORDER_RECEIPT_OVERRIDE", "Autorise l'enregistrement d'une réception de commande au-delà du reste réceptionnable (règle métier validée, Lot 6).", "Dépassement du reste réceptionnable d'une commande" }
                });

            migrationBuilder.InsertData(
                table: "import_diffs",
                columns: new[] { "id", "diff_type", "entity_id", "entity_type", "field_name", "import_batch_id", "new_value", "old_value" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0072-000000000001"), 0, new Guid("00000000-0000-0000-0022-000000000003"), "Applications", null, new Guid("00000000-0000-0000-0071-000000000001"), null, null },
                    { new Guid("00000000-0000-0000-0072-000000000002"), 1, new Guid("00000000-0000-0000-0022-000000000002"), "Applications", "Criticite", new Guid("00000000-0000-0000-0071-000000000001"), "Haute", "Moyenne" }
                });

            migrationBuilder.InsertData(
                table: "user_permissions",
                columns: new[] { "permission_id", "user_id", "granted_at", "granted_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0020-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed" },
                    { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0020-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed" },
                    { new Guid("00000000-0000-0000-0002-000000000005"), new Guid("00000000-0000-0000-0020-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed" },
                    { new Guid("00000000-0000-0000-0002-000000000006"), new Guid("00000000-0000-0000-0020-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed" },
                    { new Guid("00000000-0000-0000-0002-000000000007"), new Guid("00000000-0000-0000-0020-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_author",
                table: "audit_logs",
                column: "author");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_type_entity_id",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_timestamp",
                table: "audit_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_import_batches_import_date",
                table: "import_batches",
                column: "import_date");

            migrationBuilder.CreateIndex(
                name: "ix_import_batches_type_source_status",
                table: "import_batches",
                columns: new[] { "type", "source", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_import_diffs_import_batch_id",
                table: "import_diffs",
                column: "import_batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_receipts_order_id",
                table: "order_receipts",
                column: "order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "import_diffs");

            migrationBuilder.DropTable(
                name: "order_receipts");

            migrationBuilder.DropTable(
                name: "import_batches");

            migrationBuilder.DeleteData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0020-000000000001") });

            migrationBuilder.DeleteData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0020-000000000001") });

            migrationBuilder.DeleteData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000005"), new Guid("00000000-0000-0000-0020-000000000001") });

            migrationBuilder.DeleteData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000006"), new Guid("00000000-0000-0000-0020-000000000001") });

            migrationBuilder.DeleteData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000007"), new Guid("00000000-0000-0000-0020-000000000001") });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000003"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000004"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000005"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000006"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0002-000000000007"));

            migrationBuilder.UpdateData(
                table: "activity_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0040-000000000012"),
                column: "is_run",
                value: false);

            migrationBuilder.UpdateData(
                table: "activity_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0040-000000000013"),
                column: "is_run",
                value: false);
        }
    }
}
