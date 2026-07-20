using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafranTimeTracker.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddLot8ReferentialsAndAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "client_id",
                table: "projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "project_type_id",
                table: "projects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cost_centers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cost_centers", x => x.id);
                    table.ForeignKey(
                        name: "fk_cost_centers_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cost_centers_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_iso = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    symbole = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_currencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "technologies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technologies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "application_technologies",
                columns: table => new
                {
                    application_id = table.Column<Guid>(type: "uuid", nullable: false),
                    technology_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_technologies", x => new { x.application_id, x.technology_id });
                    table.ForeignKey(
                        name: "fk_application_technologies_application_references_application",
                        column: x => x.application_id,
                        principalTable: "application_references",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_application_technologies_technologies_technology_id",
                        column: x => x.technology_id,
                        principalTable: "technologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource_technologies",
                columns: table => new
                {
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    technology_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_technologies", x => new { x.resource_id, x.technology_id });
                    table.ForeignKey(
                        name: "fk_resource_technologies_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_technologies_technologies_technology_id",
                        column: x => x.technology_id,
                        principalTable: "technologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "clients",
                columns: new[] { "id", "code", "commentaire", "created_at", "created_by", "nom", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0081-000000000001"), "DIR-PROD", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Direction Production Applicative", 0, null, null },
                    { new Guid("00000000-0000-0000-0081-000000000002"), "DIR-SUPPORT", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Direction Support et Exploitation", 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "cost_centers",
                columns: new[] { "id", "code", "created_at", "created_by", "department_id", "libelle", "service_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0083-000000000001"), "CC-DSI", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0010-000000000001"), "Centre de coûts DSI", null, 0, null, null },
                    { new Guid("00000000-0000-0000-0083-000000000002"), "CC-PRODAPP", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0010-000000000001"), "Centre de coûts Production Applicative", new Guid("00000000-0000-0000-0011-000000000001"), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "currencies",
                columns: new[] { "id", "code_iso", "created_at", "created_by", "libelle", "statut", "symbole", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0084-000000000001"), "EUR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Euro", 0, "€", null, null },
                    { new Guid("00000000-0000-0000-0084-000000000002"), "USD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Dollar américain", 0, "$", null, null }
                });

            migrationBuilder.InsertData(
                table: "project_types",
                columns: new[] { "id", "code", "created_at", "created_by", "libelle", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0082-000000000001"), "FORFAIT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Forfait", 0, null, null },
                    { new Guid("00000000-0000-0000-0082-000000000002"), "REGIE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Régie", 0, null, null },
                    { new Guid("00000000-0000-0000-0082-000000000003"), "INTERNE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Interne", 0, null, null }
                });

            migrationBuilder.UpdateData(
                table: "projects",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0052-000000000001"),
                columns: new[] { "client_id", "project_type_id" },
                values: new object[] { new Guid("00000000-0000-0000-0081-000000000001"), new Guid("00000000-0000-0000-0082-000000000001") });

            migrationBuilder.UpdateData(
                table: "projects",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0052-000000000002"),
                columns: new[] { "client_id", "project_type_id" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "technologies",
                columns: new[] { "id", "code", "created_at", "created_by", "libelle", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0080-000000000001"), "DOTNET", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", ".NET", 0, null, null },
                    { new Guid("00000000-0000-0000-0080-000000000002"), "REACT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "React", 0, null, null },
                    { new Guid("00000000-0000-0000-0080-000000000003"), "POSTGRESQL", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "PostgreSQL", 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "application_technologies",
                columns: new[] { "application_id", "technology_id" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0022-000000000001"), new Guid("00000000-0000-0000-0080-000000000001") },
                    { new Guid("00000000-0000-0000-0022-000000000001"), new Guid("00000000-0000-0000-0080-000000000003") }
                });

            migrationBuilder.InsertData(
                table: "resource_technologies",
                columns: new[] { "resource_id", "technology_id" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0021-000000000003"), new Guid("00000000-0000-0000-0080-000000000001") },
                    { new Guid("00000000-0000-0000-0021-000000000003"), new Guid("00000000-0000-0000-0080-000000000002") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_projects_client_id",
                table: "projects",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_project_type_id",
                table: "projects",
                column: "project_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_technologies_technology_id",
                table: "application_technologies",
                column: "technology_id");

            migrationBuilder.CreateIndex(
                name: "ix_clients_code",
                table: "clients",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cost_centers_code",
                table: "cost_centers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cost_centers_department_id",
                table: "cost_centers",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_cost_centers_service_id",
                table: "cost_centers",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_currencies_code_iso",
                table: "currencies",
                column: "code_iso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_project_types_code",
                table: "project_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resource_technologies_technology_id",
                table: "resource_technologies",
                column: "technology_id");

            migrationBuilder.CreateIndex(
                name: "ix_technologies_code",
                table: "technologies",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_projects_clients_client_id",
                table: "projects",
                column: "client_id",
                principalTable: "clients",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_projects_project_types_project_type_id",
                table: "projects",
                column: "project_type_id",
                principalTable: "project_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_projects_clients_client_id",
                table: "projects");

            migrationBuilder.DropForeignKey(
                name: "fk_projects_project_types_project_type_id",
                table: "projects");

            migrationBuilder.DropTable(
                name: "application_technologies");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "cost_centers");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "project_types");

            migrationBuilder.DropTable(
                name: "resource_technologies");

            migrationBuilder.DropTable(
                name: "technologies");

            migrationBuilder.DropIndex(
                name: "ix_projects_client_id",
                table: "projects");

            migrationBuilder.DropIndex(
                name: "ix_projects_project_type_id",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "project_type_id",
                table: "projects");
        }
    }
}
