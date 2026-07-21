using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafranTimeTracker.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddLot10ProjectsSeedAndPlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "budgets",
                columns: new[] { "id", "adjusted_amount", "alert_threshold", "comment", "created_at", "created_by", "end_date", "initial_amount", "name", "order_id", "project_id", "start_date", "status", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0095-000000000001"), 80000.00m, 90m, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, 80000.00m, "Budget Refonte VTOM", null, new Guid("00000000-0000-0000-0052-000000000002"), new DateOnly(2024, 2, 1), 0, null, null });

            migrationBuilder.InsertData(
                table: "milestones",
                columns: new[] { "id", "application_id", "commentaire", "created_at", "created_by", "criticite", "date_prevue", "date_reelle", "depends_on_milestone_id", "milestone_type_id", "nom", "project_id", "responsable_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0094-000000000001"), new Guid("00000000-0000-0000-0022-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1, new DateOnly(2024, 2, 5), new DateOnly(2024, 2, 5), null, new Guid("00000000-0000-0000-0051-000000000001"), "Kick-off Refonte VTOM", new Guid("00000000-0000-0000-0052-000000000002"), new Guid("00000000-0000-0000-0021-000000000011"), 2, null, null },
                    { new Guid("00000000-0000-0000-0094-000000000002"), new Guid("00000000-0000-0000-0022-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 2, new DateOnly(2026, 10, 1), null, null, new Guid("00000000-0000-0000-0051-000000000009"), "GO PROD Refonte VTOM", new Guid("00000000-0000-0000-0052-000000000002"), new Guid("00000000-0000-0000-0021-000000000011"), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "project_participants",
                columns: new[] { "id", "capacite_prevue", "created_at", "created_by", "date_debut", "date_fin", "default_order_id", "operational_role_id", "project_id", "resource_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0091-000000000001"), 60.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0052-000000000002"), new Guid("00000000-0000-0000-0021-000000000011"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000002"), 40.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0052-000000000002"), new Guid("00000000-0000-0000-0021-000000000009"), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "project_plan_versions",
                columns: new[] { "id", "created_at", "created_by", "motif", "project_id", "statut", "type", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0092-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0052-000000000002"), 0, 0, null, null });

            migrationBuilder.InsertData(
                table: "projects",
                columns: new[] { "id", "application_id", "budget_initial", "client_id", "code", "commentaire", "created_at", "created_by", "date_debut", "date_fin_ajustee", "date_fin_prevue_initiale", "date_fin_reelle", "department_id", "description_courte", "niveau_risque", "nom", "pilote_id", "project_type_id", "service_id", "status_id", "team_id", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0090-000000000001"), new Guid("00000000-0000-0000-0022-000000000003"), 60000.00m, new Guid("00000000-0000-0000-0081-000000000002"), "PRJ-SNOW-2025", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2025, 1, 1), null, new DateOnly(2025, 9, 30), null, new Guid("00000000-0000-0000-0010-000000000001"), "Portail self-service RUN sur ServiceNow.", 2, "Portail RUN ServiceNow", new Guid("00000000-0000-0000-0021-000000000004"), new Guid("00000000-0000-0000-0082-000000000002"), new Guid("00000000-0000-0000-0011-000000000002"), new Guid("00000000-0000-0000-0050-000000000001"), new Guid("00000000-0000-0000-0012-000000000001"), null, null },
                    { new Guid("00000000-0000-0000-0090-000000000002"), new Guid("00000000-0000-0000-0022-000000000003"), 25000.00m, null, "PRJ-SNOW-SUP-2025", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2025, 3, 1), null, new DateOnly(2025, 12, 31), null, new Guid("00000000-0000-0000-0010-000000000001"), "Montée en compétence support niveau 2 sur ServiceNow.", 0, "Support ServiceNow N2", new Guid("00000000-0000-0000-0021-000000000005"), null, new Guid("00000000-0000-0000-0011-000000000003"), new Guid("00000000-0000-0000-0050-000000000001"), null, null, null },
                    { new Guid("00000000-0000-0000-0090-000000000003"), new Guid("00000000-0000-0000-0022-000000000002"), 45000.00m, null, "PRJ-VTOM-OBS-2024", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 6, 1), new DateOnly(2025, 5, 31), new DateOnly(2025, 2, 28), null, new Guid("00000000-0000-0000-0010-000000000001"), "Mise en place d'une supervision proactive de l'ordonnanceur.", 1, "Observabilité VTOM", new Guid("00000000-0000-0000-0021-000000000006"), null, new Guid("00000000-0000-0000-0011-000000000002"), new Guid("00000000-0000-0000-0050-000000000002"), new Guid("00000000-0000-0000-0012-000000000001"), null, null },
                    { new Guid("00000000-0000-0000-0090-000000000004"), new Guid("00000000-0000-0000-0022-000000000001"), 95000.00m, new Guid("00000000-0000-0000-0081-000000000001"), "PRJ-ELM-PORTAIL-2025", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2025, 2, 1), null, new DateOnly(2025, 11, 30), null, new Guid("00000000-0000-0000-0010-000000000001"), "Refonte du portail utilisateur IBM ELM.", 1, "Refonte Portail ELM", new Guid("00000000-0000-0000-0021-000000000011"), new Guid("00000000-0000-0000-0082-000000000001"), new Guid("00000000-0000-0000-0011-000000000001"), new Guid("00000000-0000-0000-0050-000000000001"), new Guid("00000000-0000-0000-0012-000000000002"), null, null },
                    { new Guid("00000000-0000-0000-0090-000000000005"), new Guid("00000000-0000-0000-0022-000000000003"), 30000.00m, null, "PRJ-REF-2024", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, new DateOnly(2024, 6, 30), new DateOnly(2024, 6, 28), new Guid("00000000-0000-0000-0010-000000000001"), "Consolidation des référentiels applicatifs dans ServiceNow CMDB.", 0, "Consolidation Référentiels", new Guid("00000000-0000-0000-0021-000000000012"), null, new Guid("00000000-0000-0000-0011-000000000004"), new Guid("00000000-0000-0000-0050-000000000003"), new Guid("00000000-0000-0000-0012-000000000002"), null, null },
                    { new Guid("00000000-0000-0000-0090-000000000006"), new Guid("00000000-0000-0000-0022-000000000002"), 15000.00m, null, "PRJ-VTOM-LEGACY-2023", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2023, 1, 1), null, new DateOnly(2023, 12, 31), new DateOnly(2023, 12, 15), new Guid("00000000-0000-0000-0010-000000000001"), "Décommissionnement de l'ancien ordonnanceur.", 0, "Archive Legacy VTOM", new Guid("00000000-0000-0000-0021-000000000013"), null, new Guid("00000000-0000-0000-0011-000000000002"), new Guid("00000000-0000-0000-0050-000000000004"), null, null, null }
                });

            migrationBuilder.InsertData(
                table: "budgets",
                columns: new[] { "id", "adjusted_amount", "alert_threshold", "comment", "created_at", "created_by", "end_date", "initial_amount", "name", "order_id", "project_id", "start_date", "status", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0095-000000000002"), 60000.00m, 90m, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, 60000.00m, "Budget Portail RUN ServiceNow", null, new Guid("00000000-0000-0000-0090-000000000001"), new DateOnly(2025, 1, 1), 0, null, null },
                    { new Guid("00000000-0000-0000-0095-000000000003"), 95000.00m, 90m, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, 95000.00m, "Budget Refonte Portail ELM", null, new Guid("00000000-0000-0000-0090-000000000004"), new DateOnly(2025, 2, 1), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "milestones",
                columns: new[] { "id", "application_id", "commentaire", "created_at", "created_by", "criticite", "date_prevue", "date_reelle", "depends_on_milestone_id", "milestone_type_id", "nom", "project_id", "responsable_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0094-000000000003"), new Guid("00000000-0000-0000-0022-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1, new DateOnly(2025, 1, 10), new DateOnly(2025, 1, 10), null, new Guid("00000000-0000-0000-0051-000000000001"), "Kick-off Portail RUN ServiceNow", new Guid("00000000-0000-0000-0090-000000000001"), new Guid("00000000-0000-0000-0021-000000000004"), 2, null, null },
                    { new Guid("00000000-0000-0000-0094-000000000004"), new Guid("00000000-0000-0000-0022-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 3, new DateOnly(2025, 6, 1), null, null, new Guid("00000000-0000-0000-0051-000000000006"), "GO QUAL Portail RUN ServiceNow", new Guid("00000000-0000-0000-0090-000000000001"), new Guid("00000000-0000-0000-0021-000000000004"), 1, null, null },
                    { new Guid("00000000-0000-0000-0094-000000000005"), new Guid("00000000-0000-0000-0022-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 0, new DateOnly(2025, 3, 5), null, null, new Guid("00000000-0000-0000-0051-000000000001"), "Kick-off Support ServiceNow N2", new Guid("00000000-0000-0000-0090-000000000002"), new Guid("00000000-0000-0000-0021-000000000005"), 0, null, null },
                    { new Guid("00000000-0000-0000-0094-000000000006"), new Guid("00000000-0000-0000-0022-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1, new DateOnly(2024, 7, 1), new DateOnly(2024, 7, 3), null, new Guid("00000000-0000-0000-0051-000000000002"), "Architecture Observabilité VTOM", new Guid("00000000-0000-0000-0090-000000000003"), new Guid("00000000-0000-0000-0021-000000000006"), 2, null, null },
                    { new Guid("00000000-0000-0000-0094-000000000008"), new Guid("00000000-0000-0000-0022-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1, new DateOnly(2025, 2, 10), new DateOnly(2025, 2, 10), null, new Guid("00000000-0000-0000-0051-000000000001"), "Kick-off Refonte Portail ELM", new Guid("00000000-0000-0000-0090-000000000004"), new Guid("00000000-0000-0000-0021-000000000011"), 2, null, null },
                    { new Guid("00000000-0000-0000-0094-000000000009"), new Guid("00000000-0000-0000-0022-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 2, new DateOnly(2025, 10, 15), null, null, new Guid("00000000-0000-0000-0051-000000000003"), "VABE Refonte Portail ELM", new Guid("00000000-0000-0000-0090-000000000004"), new Guid("00000000-0000-0000-0021-000000000011"), 0, null, null },
                    { new Guid("00000000-0000-0000-0094-000000000010"), new Guid("00000000-0000-0000-0022-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 0, new DateOnly(2024, 1, 8), new DateOnly(2024, 1, 8), null, new Guid("00000000-0000-0000-0051-000000000001"), "Kick-off Consolidation Référentiels", new Guid("00000000-0000-0000-0090-000000000005"), new Guid("00000000-0000-0000-0021-000000000012"), 2, null, null },
                    { new Guid("00000000-0000-0000-0094-000000000011"), new Guid("00000000-0000-0000-0022-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1, new DateOnly(2024, 6, 25), new DateOnly(2024, 6, 25), null, new Guid("00000000-0000-0000-0051-000000000009"), "GO PROD Consolidation Référentiels", new Guid("00000000-0000-0000-0090-000000000005"), new Guid("00000000-0000-0000-0021-000000000012"), 2, null, null },
                    { new Guid("00000000-0000-0000-0094-000000000012"), new Guid("00000000-0000-0000-0022-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 2, new DateOnly(2023, 12, 10), new DateOnly(2023, 12, 10), null, new Guid("00000000-0000-0000-0051-000000000010"), "CAB Archive Legacy VTOM", new Guid("00000000-0000-0000-0090-000000000006"), new Guid("00000000-0000-0000-0021-000000000013"), 2, null, null }
                });

            migrationBuilder.InsertData(
                table: "project_participants",
                columns: new[] { "id", "capacite_prevue", "created_at", "created_by", "date_debut", "date_fin", "default_order_id", "operational_role_id", "project_id", "resource_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0091-000000000003"), 50.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0090-000000000001"), new Guid("00000000-0000-0000-0021-000000000004"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000004"), 40.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0090-000000000001"), new Guid("00000000-0000-0000-0021-000000000009"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000005"), 30.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0090-000000000002"), new Guid("00000000-0000-0000-0021-000000000005"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000006"), 20.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, null, new Guid("00000000-0000-0000-0090-000000000002"), new Guid("00000000-0000-0000-0021-000000000010"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000007"), 35.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0090-000000000003"), new Guid("00000000-0000-0000-0021-000000000006"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000008"), 25.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0090-000000000003"), new Guid("00000000-0000-0000-0021-000000000008"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000009"), 45.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0090-000000000004"), new Guid("00000000-0000-0000-0021-000000000011"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000010"), 35.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0090-000000000004"), new Guid("00000000-0000-0000-0021-000000000012"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000011"), 20.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0090-000000000005"), new Guid("00000000-0000-0000-0021-000000000012"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000012"), 15.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, new Guid("00000000-0000-0000-0003-000000000002"), new Guid("00000000-0000-0000-0090-000000000005"), new Guid("00000000-0000-0000-0021-000000000013"), 0, null, null },
                    { new Guid("00000000-0000-0000-0091-000000000013"), 10.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2024, 1, 1), null, null, null, new Guid("00000000-0000-0000-0090-000000000006"), new Guid("00000000-0000-0000-0021-000000000013"), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "project_plan_versions",
                columns: new[] { "id", "created_at", "created_by", "motif", "project_id", "statut", "type", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0092-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0090-000000000001"), 0, 0, null, null },
                    { new Guid("00000000-0000-0000-0092-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Charge revue à la hausse suite au périmètre étendu (démonstration).", new Guid("00000000-0000-0000-0090-000000000001"), 0, 1, null, null },
                    { new Guid("00000000-0000-0000-0092-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", null, new Guid("00000000-0000-0000-0090-000000000003"), 0, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "project_weekly_plans",
                columns: new[] { "id", "charge_planifiee_heures", "created_at", "created_by", "project_plan_version_id", "resource_id", "updated_at", "updated_by", "week_start_date" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0093-000000000001"), 24.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0092-000000000001"), new Guid("00000000-0000-0000-0021-000000000011"), null, null, new DateOnly(2024, 6, 10) },
                    { new Guid("00000000-0000-0000-0093-000000000002"), 16.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0092-000000000001"), new Guid("00000000-0000-0000-0021-000000000009"), null, null, new DateOnly(2024, 6, 10) },
                    { new Guid("00000000-0000-0000-0093-000000000003"), 24.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0092-000000000001"), new Guid("00000000-0000-0000-0021-000000000011"), null, null, new DateOnly(2024, 6, 17) }
                });

            migrationBuilder.InsertData(
                table: "milestones",
                columns: new[] { "id", "application_id", "commentaire", "created_at", "created_by", "criticite", "date_prevue", "date_reelle", "depends_on_milestone_id", "milestone_type_id", "nom", "project_id", "responsable_id", "statut", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0094-000000000007"), new Guid("00000000-0000-0000-0022-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 2, new DateOnly(2025, 5, 31), null, new Guid("00000000-0000-0000-0094-000000000006"), new Guid("00000000-0000-0000-0051-000000000009"), "GO PROD Observabilité VTOM", new Guid("00000000-0000-0000-0090-000000000003"), new Guid("00000000-0000-0000-0021-000000000006"), 1, null, null });

            migrationBuilder.InsertData(
                table: "project_weekly_plans",
                columns: new[] { "id", "charge_planifiee_heures", "created_at", "created_by", "project_plan_version_id", "resource_id", "updated_at", "updated_by", "week_start_date" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0093-000000000004"), 20.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0092-000000000002"), new Guid("00000000-0000-0000-0021-000000000004"), null, null, new DateOnly(2024, 6, 10) },
                    { new Guid("00000000-0000-0000-0093-000000000005"), 28.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0092-000000000003"), new Guid("00000000-0000-0000-0021-000000000004"), null, null, new DateOnly(2024, 6, 10) },
                    { new Guid("00000000-0000-0000-0093-000000000006"), 14.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0092-000000000004"), new Guid("00000000-0000-0000-0021-000000000006"), null, null, new DateOnly(2024, 6, 10) },
                    { new Guid("00000000-0000-0000-0093-000000000007"), 12.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0092-000000000002"), new Guid("00000000-0000-0000-0021-000000000009"), null, null, new DateOnly(2024, 6, 10) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "budgets",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0095-000000000001"));

            migrationBuilder.DeleteData(
                table: "budgets",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0095-000000000002"));

            migrationBuilder.DeleteData(
                table: "budgets",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0095-000000000003"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000001"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000002"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000003"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000004"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000005"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000007"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000008"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000009"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000010"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000011"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000012"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000001"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000002"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000003"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000004"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000005"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000006"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000007"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000008"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000009"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000010"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000011"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000012"));

            migrationBuilder.DeleteData(
                table: "project_participants",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0091-000000000013"));

            migrationBuilder.DeleteData(
                table: "project_weekly_plans",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0093-000000000001"));

            migrationBuilder.DeleteData(
                table: "project_weekly_plans",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0093-000000000002"));

            migrationBuilder.DeleteData(
                table: "project_weekly_plans",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0093-000000000003"));

            migrationBuilder.DeleteData(
                table: "project_weekly_plans",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0093-000000000004"));

            migrationBuilder.DeleteData(
                table: "project_weekly_plans",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0093-000000000005"));

            migrationBuilder.DeleteData(
                table: "project_weekly_plans",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0093-000000000006"));

            migrationBuilder.DeleteData(
                table: "project_weekly_plans",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0093-000000000007"));

            migrationBuilder.DeleteData(
                table: "milestones",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0094-000000000006"));

            migrationBuilder.DeleteData(
                table: "project_plan_versions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0092-000000000001"));

            migrationBuilder.DeleteData(
                table: "project_plan_versions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0092-000000000002"));

            migrationBuilder.DeleteData(
                table: "project_plan_versions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0092-000000000003"));

            migrationBuilder.DeleteData(
                table: "project_plan_versions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0092-000000000004"));

            migrationBuilder.DeleteData(
                table: "projects",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0090-000000000002"));

            migrationBuilder.DeleteData(
                table: "projects",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0090-000000000004"));

            migrationBuilder.DeleteData(
                table: "projects",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0090-000000000005"));

            migrationBuilder.DeleteData(
                table: "projects",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0090-000000000006"));

            migrationBuilder.DeleteData(
                table: "projects",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0090-000000000001"));

            migrationBuilder.DeleteData(
                table: "projects",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0090-000000000003"));
        }
    }
}
