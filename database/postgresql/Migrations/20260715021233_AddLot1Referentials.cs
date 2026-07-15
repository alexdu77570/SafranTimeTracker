using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafranTimeTracker.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddLot1Referentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operational_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_operational_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_statuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ordre = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ordre = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    heures_par_jour = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    jours_ouvres_par_semaine = table.Column<int>(type: "integer", nullable: false),
                    pays_par_defaut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    devise_par_defaut = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    seuil_surcharge = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    seuil_sous_charge = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    seuil_alerte_budget = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    seuil_alerte_commande = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    delai_modification_temps_jours = table.Column<int>(type: "integer", nullable: false),
                    activation_validation_absences = table.Column<bool>(type: "boolean", nullable: false),
                    autorisation_saisie_sans_valorisation = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    company_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    contact_principal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email_contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    telephone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    adresse = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.ForeignKey(
                        name: "fk_companies_company_types_company_type_id",
                        column: x => x.company_type_id,
                        principalTable: "company_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    libelle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    budget_financier_initial = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    budget_jours_initial = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: true),
                    date_debut = table.Column<DateOnly>(type: "date", nullable: false),
                    date_fin_initiale = table.Column<DateOnly>(type: "date", nullable: false),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seuil_alerte = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_orders_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_orders_order_statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "order_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "application_references",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criticite = table.Column<int>(type: "integer", nullable: false),
                    responsable_id = table.Column<Guid>(type: "uuid", nullable: true),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_references", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    responsable_id = table.Column<Guid>(type: "uuid", nullable: true),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    commentaire = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_authorized_resources",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_authorized_resources", x => new { x.order_id, x.resource_id });
                    table.ForeignKey(
                        name: "fk_order_authorized_resources_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource_operational_roles",
                columns: table => new
                {
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operational_role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_operational_roles", x => new { x.resource_id, x.operational_role_id });
                    table.ForeignKey(
                        name: "fk_resource_operational_roles_operational_roles_operational_ro",
                        column: x => x.operational_role_id,
                        principalTable: "operational_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    responsable_hierarchique_id = table.Column<Guid>(type: "uuid", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    default_order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    daily_capacity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    weekly_capacity = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resources", x => x.id);
                    table.ForeignKey(
                        name: "fk_resources_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_resources_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_resources_orders_default_order_id",
                        column: x => x.default_order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_resources_resources_responsable_hierarchique_id",
                        column: x => x.responsable_hierarchique_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    responsable_id = table.Column<Guid>(type: "uuid", nullable: true),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    commentaire = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_services_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_services_resources_responsable_id",
                        column: x => x.responsable_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    identifiant = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    telephone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    date_arrivee = table.Column<DateOnly>(type: "date", nullable: false),
                    date_sortie = table.Column<DateOnly>(type: "date", nullable: true),
                    commentaire = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acces_global = table.Column<bool>(type: "boolean", nullable: false),
                    security_last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    security_last_modified_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_resources_resource_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    responsable_fonctionnel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    statut = table.Column<int>(type: "integer", nullable: false),
                    commentaire = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teams", x => x.id);
                    table.ForeignKey(
                        name: "fk_teams_resources_responsable_fonctionnel_id",
                        column: x => x.responsable_fonctionnel_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_teams_services_service_id",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_permissions",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    granted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_permissions", x => new { x.user_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_user_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_permissions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "company_types",
                columns: new[] { "id", "code", "libelle" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0004-000000000001"), "INTERNE", "Interne" },
                    { new Guid("00000000-0000-0000-0004-000000000002"), "EXTERNE", "Externe" }
                });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "id", "code", "commentaire", "created_at", "created_by", "nom", "responsable_id", "statut", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0010-000000000001"), "DSI", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Direction des Systèmes d'Information", null, 0, null, null });

            migrationBuilder.InsertData(
                table: "operational_roles",
                columns: new[] { "id", "code", "libelle" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0003-000000000001"), "RUN", "RUN" },
                    { new Guid("00000000-0000-0000-0003-000000000002"), "BUILD", "Build" },
                    { new Guid("00000000-0000-0000-0003-000000000003"), "AMELIORATION_CONTINUE", "Amélioration continue" },
                    { new Guid("00000000-0000-0000-0003-000000000004"), "CHEF_DE_PROJET", "Chef de Projet" },
                    { new Guid("00000000-0000-0000-0003-000000000005"), "COORDINATEUR_IT", "Coordinateur IT" }
                });

            migrationBuilder.InsertData(
                table: "order_statuses",
                columns: new[] { "id", "code", "libelle", "ordre" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0005-000000000001"), "BROUILLON", "Brouillon", 1 },
                    { new Guid("00000000-0000-0000-0005-000000000002"), "ACTIVE", "Active", 2 },
                    { new Guid("00000000-0000-0000-0005-000000000003"), "SUSPENDUE", "Suspendue", 3 },
                    { new Guid("00000000-0000-0000-0005-000000000004"), "CONSOMMEE", "Consommée", 4 },
                    { new Guid("00000000-0000-0000-0005-000000000005"), "CLOTUREE", "Clôturée", 5 }
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "code", "description", "libelle" },
                values: new object[] { new Guid("00000000-0000-0000-0002-000000000001"), "FINANCIAL_DATA_VIEW", "Autorise l'accès aux TJM, contrats, budgets, commandes, coûts et différentiels (cahier des charges §6.2).", "Accès aux données financières" });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "code", "libelle", "ordre" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), "INGENIEUR", "Ingénieur", 1 },
                    { new Guid("00000000-0000-0000-0001-000000000002"), "RESPONSABLE_SERVICE", "Responsable Service", 2 },
                    { new Guid("00000000-0000-0000-0001-000000000003"), "RESPONSABLE_DEPARTEMENT", "Responsable Département", 3 },
                    { new Guid("00000000-0000-0000-0001-000000000004"), "ADMINISTRATEUR", "Administrateur", 4 }
                });

            migrationBuilder.InsertData(
                table: "settings",
                columns: new[] { "id", "activation_validation_absences", "autorisation_saisie_sans_valorisation", "delai_modification_temps_jours", "devise_par_defaut", "heures_par_jour", "jours_ouvres_par_semaine", "pays_par_defaut", "seuil_alerte_budget", "seuil_alerte_commande", "seuil_sous_charge", "seuil_surcharge", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0024-000000000001"), true, false, 5, "EUR", 7.75m, 5, "France", 80m, 80m, 50m, 100m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed" });

            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "id", "adresse", "code", "commentaire", "company_type_id", "contact_principal", "created_at", "created_by", "email_contact", "nom", "statut", "telephone", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0013-000000000001"), null, "SAFRAN", "Société interne de référence (données de démonstration).", new Guid("00000000-0000-0000-0004-000000000001"), "Direction DSI", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "contact@safrantimetracker.local", "SAFRAN", 0, null, null, null });

            migrationBuilder.InsertData(
                table: "services",
                columns: new[] { "id", "code", "commentaire", "created_at", "created_by", "department_id", "nom", "responsable_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0011-000000000001"), "PRODAPP", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0010-000000000001"), "Production Applicative", null, 0, null, null },
                    { new Guid("00000000-0000-0000-0011-000000000002"), "RUNMCO", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0010-000000000001"), "RUN / MCO", null, 0, null, null },
                    { new Guid("00000000-0000-0000-0011-000000000003"), "SUPPORT", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0010-000000000001"), "Support", null, 0, null, null },
                    { new Guid("00000000-0000-0000-0011-000000000004"), "PROJETS", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new Guid("00000000-0000-0000-0010-000000000001"), "Projets", null, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "orders",
                columns: new[] { "id", "budget_financier_initial", "budget_jours_initial", "commentaire", "company_id", "created_at", "created_by", "date_debut", "date_fin_initiale", "libelle", "reference", "seuil_alerte", "status_id", "updated_at", "updated_by" },
                values: new object[] { new Guid("00000000-0000-0000-0023-000000000001"), 150000.00m, 200m, "Commande de démonstration (Lot 1).", new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "Prestation cadre SAFRAN TIME TRACKER 2026", "CMD-2026-0001", 80m, new Guid("00000000-0000-0000-0005-000000000002"), null, null });

            migrationBuilder.InsertData(
                table: "resources",
                columns: new[] { "id", "commentaire", "company_id", "created_at", "created_by", "daily_capacity", "default_order_id", "department_id", "nom", "prenom", "responsable_hierarchique_id", "service_id", "statut", "team_id", "updated_at", "updated_by", "weekly_capacity" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0021-000000000001"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "BERNARD", "Alexandre", null, new Guid("00000000-0000-0000-0011-000000000001"), 0, null, null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000002"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "LEGRAND", "Fabien", null, new Guid("00000000-0000-0000-0011-000000000001"), 0, null, null, null, 38.75m }
                });

            migrationBuilder.InsertData(
                table: "teams",
                columns: new[] { "id", "code", "commentaire", "created_at", "created_by", "nom", "responsable_fonctionnel_id", "service_id", "statut", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0012-000000000001"), "RUN-A", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Équipe RUN A", null, new Guid("00000000-0000-0000-0011-000000000002"), 0, null, null },
                    { new Guid("00000000-0000-0000-0012-000000000002"), "PROJ-A", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", "Équipe Projets A", null, new Guid("00000000-0000-0000-0011-000000000004"), 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "resources",
                columns: new[] { "id", "commentaire", "company_id", "created_at", "created_by", "daily_capacity", "default_order_id", "department_id", "nom", "prenom", "responsable_hierarchique_id", "service_id", "statut", "team_id", "updated_at", "updated_by", "weekly_capacity" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0021-000000000003"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "GEORGES", "Thierry", new Guid("00000000-0000-0000-0021-000000000002"), new Guid("00000000-0000-0000-0011-000000000001"), 0, null, null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000004"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "MANCERON", "Emmanuel", new Guid("00000000-0000-0000-0021-000000000002"), new Guid("00000000-0000-0000-0011-000000000002"), 0, null, null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000005"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "FOCQUENOEY", "Thomas", new Guid("00000000-0000-0000-0021-000000000002"), new Guid("00000000-0000-0000-0011-000000000003"), 0, null, null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000006"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "REAU", "Alexandre", new Guid("00000000-0000-0000-0021-000000000002"), new Guid("00000000-0000-0000-0011-000000000004"), 0, null, null, null, 38.75m }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "acces_global", "commentaire", "created_at", "created_by", "date_arrivee", "date_sortie", "email", "identifiant", "nom", "prenom", "resource_id", "role_id", "security_last_modified_at", "security_last_modified_by", "statut", "telephone", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0020-000000000001"), true, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "s636140@safrantimetracker.local", "s636140", "BERNARD", "Alexandre", new Guid("00000000-0000-0000-0021-000000000001"), new Guid("00000000-0000-0000-0001-000000000004"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000002"), true, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "flegrand@safrantimetracker.local", "flegrand", "LEGRAND", "Fabien", new Guid("00000000-0000-0000-0021-000000000002"), new Guid("00000000-0000-0000-0001-000000000003"), null, null, 0, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "application_references",
                columns: new[] { "id", "code", "commentaire", "created_at", "created_by", "criticite", "nom", "responsable_id", "service_id", "statut", "team_id", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0022-000000000001"), "IBMELM", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 2, "IBM ELM", new Guid("00000000-0000-0000-0021-000000000003"), new Guid("00000000-0000-0000-0011-000000000001"), 0, null, null, null },
                    { new Guid("00000000-0000-0000-0022-000000000002"), "VTOM", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 1, "VTOM", new Guid("00000000-0000-0000-0021-000000000004"), new Guid("00000000-0000-0000-0011-000000000002"), 0, new Guid("00000000-0000-0000-0012-000000000001"), null, null },
                    { new Guid("00000000-0000-0000-0022-000000000003"), "SNOW", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 3, "ServiceNow", new Guid("00000000-0000-0000-0021-000000000005"), new Guid("00000000-0000-0000-0011-000000000003"), 0, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "order_authorized_resources",
                columns: new[] { "order_id", "resource_id" },
                values: new object[] { new Guid("00000000-0000-0000-0023-000000000001"), new Guid("00000000-0000-0000-0021-000000000003") });

            migrationBuilder.InsertData(
                table: "resource_operational_roles",
                columns: new[] { "operational_role_id", "resource_id" },
                values: new object[] { new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0021-000000000003") });

            migrationBuilder.InsertData(
                table: "resources",
                columns: new[] { "id", "commentaire", "company_id", "created_at", "created_by", "daily_capacity", "default_order_id", "department_id", "nom", "prenom", "responsable_hierarchique_id", "service_id", "statut", "team_id", "updated_at", "updated_by", "weekly_capacity" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0021-000000000007"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "MISHRA", "Reena", new Guid("00000000-0000-0000-0021-000000000003"), new Guid("00000000-0000-0000-0011-000000000001"), 0, null, null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000008"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "DURAND", "Camille", new Guid("00000000-0000-0000-0021-000000000004"), new Guid("00000000-0000-0000-0011-000000000002"), 0, new Guid("00000000-0000-0000-0012-000000000001"), null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000009"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "NGUYEN", "Minh", new Guid("00000000-0000-0000-0021-000000000004"), new Guid("00000000-0000-0000-0011-000000000002"), 0, new Guid("00000000-0000-0000-0012-000000000001"), null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000010"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "PATEL", "Aarav", new Guid("00000000-0000-0000-0021-000000000005"), new Guid("00000000-0000-0000-0011-000000000003"), 0, null, null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000011"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "LEFEVRE", "Julie", new Guid("00000000-0000-0000-0021-000000000006"), new Guid("00000000-0000-0000-0011-000000000004"), 0, new Guid("00000000-0000-0000-0012-000000000002"), null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000012"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "COSTA", "Marco", new Guid("00000000-0000-0000-0021-000000000006"), new Guid("00000000-0000-0000-0011-000000000004"), 0, new Guid("00000000-0000-0000-0012-000000000002"), null, null, 38.75m },
                    { new Guid("00000000-0000-0000-0021-000000000013"), null, new Guid("00000000-0000-0000-0013-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", 7.75m, null, new Guid("00000000-0000-0000-0010-000000000001"), "VERMA", "Priya", new Guid("00000000-0000-0000-0021-000000000003"), new Guid("00000000-0000-0000-0011-000000000001"), 0, null, null, null, 38.75m }
                });

            migrationBuilder.InsertData(
                table: "user_permissions",
                columns: new[] { "permission_id", "user_id", "granted_at", "granted_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0020-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed" },
                    { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0020-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "acces_global", "commentaire", "created_at", "created_by", "date_arrivee", "date_sortie", "email", "identifiant", "nom", "prenom", "resource_id", "role_id", "security_last_modified_at", "security_last_modified_by", "statut", "telephone", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0020-000000000003"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "tgeorges@safrantimetracker.local", "tgeorges", "GEORGES", "Thierry", new Guid("00000000-0000-0000-0021-000000000003"), new Guid("00000000-0000-0000-0001-000000000002"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000004"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "emanceron@safrantimetracker.local", "emanceron", "MANCERON", "Emmanuel", new Guid("00000000-0000-0000-0021-000000000004"), new Guid("00000000-0000-0000-0001-000000000002"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000005"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "tfocquenoey@safrantimetracker.local", "tfocquenoey", "FOCQUENOEY", "Thomas", new Guid("00000000-0000-0000-0021-000000000005"), new Guid("00000000-0000-0000-0001-000000000002"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000006"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "areau@safrantimetracker.local", "areau", "REAU", "Alexandre", new Guid("00000000-0000-0000-0021-000000000006"), new Guid("00000000-0000-0000-0001-000000000002"), null, null, 0, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "order_authorized_resources",
                columns: new[] { "order_id", "resource_id" },
                values: new object[] { new Guid("00000000-0000-0000-0023-000000000001"), new Guid("00000000-0000-0000-0021-000000000007") });

            migrationBuilder.InsertData(
                table: "resource_operational_roles",
                columns: new[] { "operational_role_id", "resource_id" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0021-000000000007") },
                    { new Guid("00000000-0000-0000-0003-000000000001"), new Guid("00000000-0000-0000-0021-000000000008") },
                    { new Guid("00000000-0000-0000-0003-000000000004"), new Guid("00000000-0000-0000-0021-000000000011") }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "acces_global", "commentaire", "created_at", "created_by", "date_arrivee", "date_sortie", "email", "identifiant", "nom", "prenom", "resource_id", "role_id", "security_last_modified_at", "security_last_modified_by", "statut", "telephone", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0020-000000000007"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "rmishra@safrantimetracker.local", "rmishra", "MISHRA", "Reena", new Guid("00000000-0000-0000-0021-000000000007"), new Guid("00000000-0000-0000-0001-000000000001"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000008"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "cdurand@safrantimetracker.local", "cdurand", "DURAND", "Camille", new Guid("00000000-0000-0000-0021-000000000008"), new Guid("00000000-0000-0000-0001-000000000001"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000009"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "mnguyen@safrantimetracker.local", "mnguyen", "NGUYEN", "Minh", new Guid("00000000-0000-0000-0021-000000000009"), new Guid("00000000-0000-0000-0001-000000000001"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000010"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "apatel@safrantimetracker.local", "apatel", "PATEL", "Aarav", new Guid("00000000-0000-0000-0021-000000000010"), new Guid("00000000-0000-0000-0001-000000000001"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000011"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "jlefevre@safrantimetracker.local", "jlefevre", "LEFEVRE", "Julie", new Guid("00000000-0000-0000-0021-000000000011"), new Guid("00000000-0000-0000-0001-000000000001"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000012"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "mcosta@safrantimetracker.local", "mcosta", "COSTA", "Marco", new Guid("00000000-0000-0000-0021-000000000012"), new Guid("00000000-0000-0000-0001-000000000001"), null, null, 0, null, null, null },
                    { new Guid("00000000-0000-0000-0020-000000000013"), false, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system-seed", new DateOnly(2021, 9, 1), null, "pverma@safrantimetracker.local", "pverma", "VERMA", "Priya", new Guid("00000000-0000-0000-0021-000000000013"), new Guid("00000000-0000-0000-0001-000000000001"), null, null, 0, null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_application_references_code",
                table: "application_references",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_application_references_responsable_id",
                table: "application_references",
                column: "responsable_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_references_service_id",
                table: "application_references",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_application_references_team_id",
                table: "application_references",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_code",
                table: "companies",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_companies_company_type_id",
                table: "companies",
                column: "company_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_types_code",
                table: "company_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_code",
                table: "departments",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_responsable_id",
                table: "departments",
                column: "responsable_id");

            migrationBuilder.CreateIndex(
                name: "ix_operational_roles_code",
                table: "operational_roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_authorized_resources_resource_id",
                table: "order_authorized_resources",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_statuses_code",
                table: "order_statuses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_company_id",
                table: "orders",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_reference",
                table: "orders",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_status_id",
                table: "orders",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_code",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resource_operational_roles_operational_role_id",
                table: "resource_operational_roles",
                column: "operational_role_id");

            migrationBuilder.CreateIndex(
                name: "ix_resources_company_id",
                table: "resources",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_resources_default_order_id",
                table: "resources",
                column: "default_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_resources_department_id",
                table: "resources",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_resources_responsable_hierarchique_id",
                table: "resources",
                column: "responsable_hierarchique_id");

            migrationBuilder.CreateIndex(
                name: "ix_resources_service_id",
                table: "resources",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_resources_team_id",
                table: "resources",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "ix_roles_code",
                table: "roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_services_code",
                table: "services",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_services_department_id",
                table: "services",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_services_responsable_id",
                table: "services",
                column: "responsable_id");

            migrationBuilder.CreateIndex(
                name: "ix_teams_code",
                table: "teams",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teams_responsable_fonctionnel_id",
                table: "teams",
                column: "responsable_fonctionnel_id");

            migrationBuilder.CreateIndex(
                name: "ix_teams_service_id",
                table: "teams",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_permissions_permission_id",
                table: "user_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_identifiant",
                table: "users",
                column: "identifiant",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_resource_id",
                table: "users",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.AddForeignKey(
                name: "fk_application_references_resources_responsable_id",
                table: "application_references",
                column: "responsable_id",
                principalTable: "resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_application_references_services_service_id",
                table: "application_references",
                column: "service_id",
                principalTable: "services",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_application_references_teams_team_id",
                table: "application_references",
                column: "team_id",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_departments_resources_responsable_id",
                table: "departments",
                column: "responsable_id",
                principalTable: "resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_order_authorized_resources_resources_resource_id",
                table: "order_authorized_resources",
                column: "resource_id",
                principalTable: "resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_resource_operational_roles_resources_resource_id",
                table: "resource_operational_roles",
                column: "resource_id",
                principalTable: "resources",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_resources_services_service_id",
                table: "resources",
                column: "service_id",
                principalTable: "services",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_resources_teams_team_id",
                table: "resources",
                column: "team_id",
                principalTable: "teams",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_departments_resources_responsable_id",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "fk_services_resources_responsable_id",
                table: "services");

            migrationBuilder.DropForeignKey(
                name: "fk_teams_resources_responsable_fonctionnel_id",
                table: "teams");

            migrationBuilder.DropTable(
                name: "application_references");

            migrationBuilder.DropTable(
                name: "order_authorized_resources");

            migrationBuilder.DropTable(
                name: "resource_operational_roles");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "user_permissions");

            migrationBuilder.DropTable(
                name: "operational_roles");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "resources");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "order_statuses");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "company_types");

            migrationBuilder.DropTable(
                name: "departments");
        }
    }
}
