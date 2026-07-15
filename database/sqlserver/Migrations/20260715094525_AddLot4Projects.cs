using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafranTimeTracker.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddLot4Projects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "time_entries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "milestone_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    statut = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_milestone_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_statuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ordre = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description_courte = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    pilote_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    department_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    service_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    team_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date_debut = table.Column<DateOnly>(type: "date", nullable: false),
                    date_fin_prevue_initiale = table.Column<DateOnly>(type: "date", nullable: false),
                    date_fin_ajustee = table.Column<DateOnly>(type: "date", nullable: true),
                    date_fin_reelle = table.Column<DateOnly>(type: "date", nullable: true),
                    budget_initial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    niveau_risque = table.Column<int>(type: "int", nullable: false),
                    commentaire = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_application_references_application_id",
                        column: x => x.application_id,
                        principalTable: "application_references",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_projects_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_projects_project_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "project_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_projects_resources_pilote_id",
                        column: x => x.pilote_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_projects_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_projects_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "milestones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    milestone_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    application_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    responsable_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date_prevue = table.Column<DateOnly>(type: "date", nullable: false),
                    date_reelle = table.Column<DateOnly>(type: "date", nullable: true),
                    statut = table.Column<int>(type: "int", nullable: false),
                    criticite = table.Column<int>(type: "int", nullable: false),
                    commentaire = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    depends_on_milestone_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_milestones", x => x.id);
                    table.ForeignKey(
                        name: "fk_milestones_application_references_application_id",
                        column: x => x.application_id,
                        principalTable: "application_references",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_milestones_milestone_types_milestone_type_id",
                        column: x => x.milestone_type_id,
                        principalTable: "milestone_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_milestones_milestones_depends_on_milestone_id",
                        column: x => x.depends_on_milestone_id,
                        principalTable: "milestones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_milestones_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_milestones_resources_responsable_id",
                        column: x => x.responsable_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_participants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    operational_role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    default_order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    date_debut = table.Column<DateOnly>(type: "date", nullable: false),
                    date_fin = table.Column<DateOnly>(type: "date", nullable: true),
                    capacite_prevue = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    statut = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_participants", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_participants_operational_roles_operational_role_id",
                        column: x => x.operational_role_id,
                        principalTable: "operational_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_project_participants_orders_default_order_id",
                        column: x => x.default_order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_project_participants_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_project_participants_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_plan_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    statut = table.Column<int>(type: "int", nullable: false),
                    motif = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_plan_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_plan_versions_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_weekly_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    project_plan_version_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    resource_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    week_start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    charge_planifiee_heures = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_weekly_plans", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_weekly_plans_project_plan_versions_project_plan_version_id",
                        column: x => x.project_plan_version_id,
                        principalTable: "project_plan_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_project_weekly_plans_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "milestone_types",
                columns: new[] { "id", "code", "created_at", "created_by", "libelle", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0051-000000000001"), "KICKOFF", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Kick-off", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000002"), "ARCHITECTURE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Architecture", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000003"), "VABE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "VABE", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000004"), "VSR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "VSR", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000005"), "GO_DEV", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "GO DEV", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000006"), "GO_QUAL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "GO QUAL", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000007"), "GO_VAL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "GO VAL", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000008"), "GO_PPROD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "GO PPROD", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000009"), "GO_PROD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "GO PROD", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000010"), "CAB", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "CAB", 0, null, null },
                    { new Guid("00000000-0000-0000-0051-000000000011"), "HYPERCARE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Hypercare", 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "project_statuses",
                columns: new[] { "id", "code", "libelle", "ordre" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0050-000000000001"), "ACTIF", "Actif", 1 },
                    { new Guid("00000000-0000-0000-0050-000000000002"), "SUSPENDU", "Suspendu", 2 },
                    { new Guid("00000000-0000-0000-0050-000000000003"), "TERMINE", "Terminé", 3 },
                    { new Guid("00000000-0000-0000-0050-000000000004"), "ARCHIVE", "Archivé", 4 }
                });

            migrationBuilder.UpdateData(
                table: "time_entries",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0043-000000000001"),
                column: "project_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "time_entries",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0043-000000000002"),
                column: "project_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "time_entries",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0043-000000000003"),
                column: "project_id",
                value: new Guid("00000000-0000-0000-0052-000000000001"));

            migrationBuilder.UpdateData(
                table: "time_entries",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0043-000000000004"),
                column: "project_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "time_entries",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0043-000000000005"),
                column: "project_id",
                value: null);

            migrationBuilder.UpdateData(
                table: "time_entries",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0043-000000000006"),
                column: "project_id",
                value: null);

            migrationBuilder.InsertData(
                table: "projects",
                columns: new[] { "id", "application_id", "budget_initial", "code", "commentaire", "created_at", "created_by", "date_debut", "date_fin_ajustee", "date_fin_prevue_initiale", "date_fin_reelle", "department_id", "description_courte", "niveau_risque", "nom", "pilote_id", "service_id", "status_id", "team_id", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0052-000000000001"), new Guid("00000000-0000-0000-0022-000000000001"), 150000.00m, "PRJ-ELM-2026", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), new DateOnly(2025, 3, 31), new DateOnly(2024, 12, 31), null, new Guid("00000000-0000-0000-0010-000000000001"), "Migration de la plateforme IBM ELM.", 1, "Migration ELM", new Guid("00000000-0000-0000-0021-000000000003"), new Guid("00000000-0000-0000-0011-000000000004"), new Guid("00000000-0000-0000-0050-000000000001"), new Guid("00000000-0000-0000-0012-000000000002"), null, null },
                    { new Guid("00000000-0000-0000-0052-000000000002"), new Guid("00000000-0000-0000-0022-000000000002"), 80000.00m, "PRJ-VTOM-2026", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 2, 1), null, new DateOnly(2024, 11, 30), null, new Guid("00000000-0000-0000-0010-000000000001"), "Refonte de l'ordonnanceur VTOM.", 0, "Refonte VTOM", new Guid("00000000-0000-0000-0021-000000000011"), new Guid("00000000-0000-0000-0011-000000000002"), new Guid("00000000-0000-0000-0050-000000000001"), new Guid("00000000-0000-0000-0012-000000000001"), null, null }
                });

            migrationBuilder.InsertData(
                table: "milestones",
                columns: new[] { "id", "application_id", "commentaire", "created_at", "created_by", "criticite", "date_prevue", "date_reelle", "depends_on_milestone_id", "milestone_type_id", "nom", "project_id", "responsable_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0056-000000000001"), new Guid("00000000-0000-0000-0022-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1, new DateOnly(2024, 1, 15), new DateOnly(2024, 1, 15), null, new Guid("00000000-0000-0000-0051-000000000001"), "Kick-off Migration ELM", new Guid("00000000-0000-0000-0052-000000000001"), new Guid("00000000-0000-0000-0021-000000000003"), 2, null, null },
                    { new Guid("00000000-0000-0000-0056-000000000002"), new Guid("00000000-0000-0000-0022-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 3, new DateOnly(2025, 6, 1), null, null, new Guid("00000000-0000-0000-0051-000000000009"), "GO PROD Migration ELM", new Guid("00000000-0000-0000-0052-000000000001"), new Guid("00000000-0000-0000-0021-000000000003"), 1, null, null },
                    { new Guid("00000000-0000-0000-0056-000000000003"), new Guid("00000000-0000-0000-0022-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 2, new DateOnly(2027, 1, 1), null, null, new Guid("00000000-0000-0000-0051-000000000010"), "CAB Migration ELM", new Guid("00000000-0000-0000-0052-000000000001"), new Guid("00000000-0000-0000-0021-000000000003"), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "project_participants",
                columns: new[] { "id", "capacite_prevue", "created_at", "created_by", "date_debut", "date_fin", "default_order_id", "operational_role_id", "project_id", "resource_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0053-000000000001"), 100.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0052-000000000001"), new Guid("00000000-0000-0000-0021-000000000003"), 0, null, null },
                    { new Guid("00000000-0000-0000-0053-000000000002"), 50.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, null, new Guid("00000000-0000-0000-0052-000000000001"), new Guid("00000000-0000-0000-0021-000000000002"), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "project_plan_versions",
                columns: new[] { "id", "created_at", "created_by", "motif", "project_id", "statut", "type", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0054-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0052-000000000001"), 0, 0, null, null },
                    { new Guid("00000000-0000-0000-0054-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Report de la fin de projet, charge revue à la hausse (démonstration).", new Guid("00000000-0000-0000-0052-000000000001"), 0, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "project_weekly_plans",
                columns: new[] { "id", "charge_planifiee_heures", "created_at", "created_by", "project_plan_version_id", "resource_id", "updated_at", "updated_by", "week_start_date" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0055-000000000001"), 20.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0054-000000000001"), new Guid("00000000-0000-0000-0021-000000000003"), null, null, new DateOnly(2024, 6, 10) },
                    { new Guid("00000000-0000-0000-0055-000000000002"), 10.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0054-000000000001"), new Guid("00000000-0000-0000-0021-000000000002"), null, null, new DateOnly(2024, 6, 10) },
                    { new Guid("00000000-0000-0000-0055-000000000003"), 24.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0054-000000000002"), new Guid("00000000-0000-0000-0021-000000000003"), null, null, new DateOnly(2024, 6, 10) },
                    { new Guid("00000000-0000-0000-0055-000000000004"), 8.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0054-000000000002"), new Guid("00000000-0000-0000-0021-000000000002"), null, null, new DateOnly(2024, 6, 10) }
                });

            migrationBuilder.CreateIndex(
                name: "ix_time_entries_project_id",
                table: "time_entries",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_milestone_types_code",
                table: "milestone_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_milestones_application_id",
                table: "milestones",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "ix_milestones_depends_on_milestone_id",
                table: "milestones",
                column: "depends_on_milestone_id");

            migrationBuilder.CreateIndex(
                name: "ix_milestones_milestone_type_id",
                table: "milestones",
                column: "milestone_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_milestones_project_id_date_prevue",
                table: "milestones",
                columns: new[] { "project_id", "date_prevue" });

            migrationBuilder.CreateIndex(
                name: "ix_milestones_responsable_id",
                table: "milestones",
                column: "responsable_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_participants_default_order_id",
                table: "project_participants",
                column: "default_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_participants_operational_role_id",
                table: "project_participants",
                column: "operational_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_participants_project_id_resource_id",
                table: "project_participants",
                columns: new[] { "project_id", "resource_id" });

            migrationBuilder.CreateIndex(
                name: "ix_project_participants_resource_id",
                table: "project_participants",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_plan_versions_project_id_type_statut",
                table: "project_plan_versions",
                columns: new[] { "project_id", "type", "statut" });

            migrationBuilder.CreateIndex(
                name: "ix_project_statuses_code",
                table: "project_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_weekly_plans_project_plan_version_id_resource_id_week_start_date",
                table: "project_weekly_plans",
                columns: new[] { "project_plan_version_id", "resource_id", "week_start_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_weekly_plans_resource_id",
                table: "project_weekly_plans",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_application_id",
                table: "projects",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_code",
                table: "projects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_projects_department_id",
                table: "projects",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_pilote_id",
                table: "projects",
                column: "pilote_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_service_id",
                table: "projects",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_status_id",
                table: "projects",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_team_id",
                table: "projects",
                column: "team_id");

            migrationBuilder.AddForeignKey(
                name: "fk_time_entries_projects_project_id",
                table: "time_entries",
                column: "project_id",
                principalTable: "projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_time_entries_projects_project_id",
                table: "time_entries");

            migrationBuilder.DropTable(
                name: "milestones");

            migrationBuilder.DropTable(
                name: "project_participants");

            migrationBuilder.DropTable(
                name: "project_weekly_plans");

            migrationBuilder.DropTable(
                name: "milestone_types");

            migrationBuilder.DropTable(
                name: "project_plan_versions");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "project_statuses");

            migrationBuilder.DropIndex(
                name: "ix_time_entries_project_id",
                table: "time_entries");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "time_entries");
        }
    }
}
