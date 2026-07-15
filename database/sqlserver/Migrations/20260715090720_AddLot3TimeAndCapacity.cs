using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafranTimeTracker.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddLot3TimeAndCapacity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "absences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    date_debut = table.Column<DateOnly>(type: "date", nullable: false),
                    date_fin = table.Column<DateOnly>(type: "date", nullable: false),
                    demi_journee = table.Column<bool>(type: "bit", nullable: false),
                    commentaire = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    statut = table.Column<int>(type: "int", nullable: false),
                    valide_par_identifiant = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    date_decision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_absences", x => x.id);
                    table.ForeignKey(
                        name: "fk_absences_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "activity_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    is_run = table.Column<bool>(type: "bit", nullable: false),
                    reference_required = table.Column<bool>(type: "bit", nullable: false),
                    reference_format_regex = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    reference_example = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    statut = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "holiday_calendar",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    pays = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    statut = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_holiday_calendar", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resource_capacity_periods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    daily_capacity = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    weekly_capacity = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_capacity_periods", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_capacity_periods_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "time_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    activity_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    duree_heures = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    commentaire = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    statut = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_time_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_time_entries_activity_types_activity_type_id",
                        column: x => x.activity_type_id,
                        principalTable: "activity_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_time_entries_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_time_entries_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "time_entry_financial_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    time_entry_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tjm_personne_snapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    source_tjm_personne = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    resource_tjm_history_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    tjm_contrat_snapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    source_contrat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    company_contract_history_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    company_id_snapshot = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    cout_reel_calcule = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    cout_contrat_calcule = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    differentiel_calcule = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    calculation_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    calculation_status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_time_entry_financial_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_time_entry_financial_snapshots_time_entries_time_entry_id",
                        column: x => x.time_entry_id,
                        principalTable: "time_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "absences",
                columns: new[] { "id", "commentaire", "created_at", "created_by", "date_debut", "date_decision", "date_fin", "demi_journee", "resource_id", "statut", "type", "updated_at", "updated_by", "valide_par_identifiant" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0045-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 7, 1), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 7, 5), false, new Guid("00000000-0000-0000-0021-000000000001"), 2, 0, null, null, "flegrand" },
                    { new Guid("00000000-0000-0000-0045-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 8, 12), null, new DateOnly(2024, 8, 12), true, new Guid("00000000-0000-0000-0021-000000000002"), 1, 2, null, null, null },
                    { new Guid("00000000-0000-0000-0045-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 9, 2), null, new DateOnly(2024, 9, 2), false, new Guid("00000000-0000-0000-0021-000000000003"), 0, 1, null, null, null },
                    { new Guid("00000000-0000-0000-0045-000000000004"), "Formation déjà suivie récemment.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 5, 10), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2024, 5, 10), false, new Guid("00000000-0000-0000-0021-000000000007"), 3, 3, null, null, "tgeorges" }
                });

            migrationBuilder.InsertData(
                table: "activity_types",
                columns: new[] { "id", "code", "created_at", "created_by", "is_run", "libelle", "reference_example", "reference_format_regex", "reference_required", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0040-000000000001"), "RUN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", true, "RUN", null, null, false, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000002"), "INCIDENT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", true, "Incident", "INC0001234", "^INC\\d{7}$", true, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000003"), "CHANGE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", true, "Change", "CHG0001234", "^CHG\\d{7}$", true, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000004"), "PROBLEM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", true, "Problem", "PRB0001234", "^PRB\\d{7}$", true, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000005"), "RITM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", true, "RITM", "RITM0001234", "^RITM\\d{7}$", true, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000006"), "PROJET", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", false, "Projet", null, null, false, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000007"), "AMELIORATION_CONTINUE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", false, "Amélioration continue", null, null, false, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000008"), "SUPPORT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", true, "Support", null, null, false, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000009"), "ASTREINTE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", true, "Astreinte", null, null, false, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000010"), "REUNION", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", false, "Réunion", null, null, false, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000011"), "FORMATION", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", false, "Formation", null, null, false, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000012"), "VABE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", false, "VABE", "VABE-0012", "^VABE-\\d{4}$", true, 0, null, null },
                    { new Guid("00000000-0000-0000-0040-000000000013"), "VSR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", false, "VSR", "VSR-0012", "^VSR-\\d{4}$", true, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "holiday_calendar",
                columns: new[] { "id", "created_at", "created_by", "date", "libelle", "pays", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0041-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), "Jour de l'an", "France", 0, null, null },
                    { new Guid("00000000-0000-0000-0041-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 5, 1), "Fête du travail", "France", 0, null, null },
                    { new Guid("00000000-0000-0000-0041-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 7, 14), "Fête nationale", "France", 0, null, null },
                    { new Guid("00000000-0000-0000-0041-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 12, 25), "Noël", "France", 0, null, null },
                    { new Guid("00000000-0000-0000-0041-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2025, 1, 1), "Jour de l'an", "France", 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "resource_capacity_periods",
                columns: new[] { "id", "created_at", "created_by", "daily_capacity", "end_date", "reason", "resource_id", "start_date", "status", "updated_at", "updated_by", "weekly_capacity" },
                values: new object[] { new Guid("00000000-0000-0000-0042-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 4.00m, null, "Temps partiel (démonstration, cahier des charges §10.5).", new Guid("00000000-0000-0000-0021-000000000010"), new DateOnly(2024, 1, 1), 0, null, null, 20.00m });

            migrationBuilder.InsertData(
                table: "resources",
                columns: new[] { "id", "commentaire", "company_id", "created_at", "created_by", "daily_capacity", "default_order_id", "department_id", "nom", "prenom", "responsable_hierarchique_id", "service_id", "statut", "team_id", "updated_at", "updated_by", "weekly_capacity" },
                values: new object[] { new Guid("00000000-0000-0000-0046-000000000001"), "Ressource désactivée de démonstration (test de blocage §19.4).", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "RESSOURCE-INACTIVE", "Démonstration", null, new Guid("00000000-0000-0000-0011-000000000001"), 1, null, null, null, 38.75m });

            migrationBuilder.InsertData(
                table: "time_entries",
                columns: new[] { "id", "activity_type_id", "commentaire", "created_at", "created_by", "date", "duree_heures", "order_id", "reference", "resource_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0043-000000000001"), new Guid("00000000-0000-0000-0040-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 6, 10), 7.75m, null, null, new Guid("00000000-0000-0000-0021-000000000001"), 0, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000002"), new Guid("00000000-0000-0000-0040-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 6, 11), 7.75m, new Guid("00000000-0000-0000-0023-000000000001"), "INC0001234", new Guid("00000000-0000-0000-0021-000000000001"), 0, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000003"), new Guid("00000000-0000-0000-0040-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 6, 10), 7.75m, null, null, new Guid("00000000-0000-0000-0021-000000000002"), 0, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000004"), new Guid("00000000-0000-0000-0040-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 3, 15), 7.75m, null, "CHG0001234", new Guid("00000000-0000-0000-0021-000000000003"), 0, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000005"), new Guid("00000000-0000-0000-0040-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2025, 3, 15), 7.75m, null, "CHG0009999", new Guid("00000000-0000-0000-0021-000000000003"), 0, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000006"), new Guid("00000000-0000-0000-0040-000000000011"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 6, 10), 7.75m, null, null, new Guid("00000000-0000-0000-0021-000000000007"), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "time_entry_financial_snapshots",
                columns: new[] { "id", "calculation_date", "calculation_status", "company_contract_history_id", "company_id_snapshot", "cout_contrat_calcule", "cout_reel_calcule", "created_at", "created_by", "differentiel_calcule", "resource_tjm_history_id", "source_contrat", "source_tjm_personne", "time_entry_id", "tjm_contrat_snapshot", "tjm_personne_snapshot", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0043-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, new Guid("00000000-0000-0000-0013-000000000001"), null, 650.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0030-000000000001"), null, "ResourceTjmHistory", new Guid("00000000-0000-0000-0043-000000000001"), null, 650.00m, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, new Guid("00000000-0000-0000-0013-000000000001"), null, 650.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0030-000000000001"), null, "ResourceTjmHistory", new Guid("00000000-0000-0000-0043-000000000002"), null, 650.00m, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, new Guid("00000000-0000-0000-0031-000000000001"), new Guid("00000000-0000-0000-0013-000000000002"), 750.00m, 700.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 50.00m, new Guid("00000000-0000-0000-0030-000000000002"), "CompanyContractHistory", "ResourceTjmHistory", new Guid("00000000-0000-0000-0043-000000000003"), 750.00m, 700.00m, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, new Guid("00000000-0000-0000-0013-000000000001"), null, 600.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0030-000000000003"), null, "ResourceTjmHistory", new Guid("00000000-0000-0000-0043-000000000004"), null, 600.00m, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, new Guid("00000000-0000-0000-0013-000000000001"), null, 620.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0030-000000000004"), null, "ResourceTjmHistory", new Guid("00000000-0000-0000-0043-000000000005"), null, 620.00m, null, null },
                    { new Guid("00000000-0000-0000-0043-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, null, null, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, null, null, null, new Guid("00000000-0000-0000-0043-000000000006"), null, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_absences_resource_id_date_debut",
                table: "absences",
                columns: new[] { "resource_id", "date_debut" });

            migrationBuilder.CreateIndex(
                name: "ix_activity_types_code",
                table: "activity_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_holiday_calendar_pays_date",
                table: "holiday_calendar",
                columns: new[] { "pays", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resource_capacity_periods_resource_id_start_date",
                table: "resource_capacity_periods",
                columns: new[] { "resource_id", "start_date" });

            migrationBuilder.CreateIndex(
                name: "ix_time_entries_activity_type_id",
                table: "time_entries",
                column: "activity_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_time_entries_order_id",
                table: "time_entries",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_time_entries_resource_id_date",
                table: "time_entries",
                columns: new[] { "resource_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_time_entry_financial_snapshots_time_entry_id",
                table: "time_entry_financial_snapshots",
                column: "time_entry_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "absences");

            migrationBuilder.DropTable(
                name: "holiday_calendar");

            migrationBuilder.DropTable(
                name: "resource_capacity_periods");

            migrationBuilder.DropTable(
                name: "time_entry_financial_snapshots");

            migrationBuilder.DropTable(
                name: "time_entries");

            migrationBuilder.DropTable(
                name: "activity_types");

            migrationBuilder.DeleteData(
                table: "resources",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0046-000000000001"));
        }
    }
}
