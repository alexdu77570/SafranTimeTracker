using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafranTimeTracker.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddLot13AuthenticationRbac : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "effect",
                table: "user_permissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_persistent = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_activity_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0001-000000000004") },
                    { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0001-000000000004") },
                    { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0001-000000000004") },
                    { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0001-000000000004") },
                    { new Guid("00000000-0000-0000-0002-000000000005"), new Guid("00000000-0000-0000-0001-000000000004") },
                    { new Guid("00000000-0000-0000-0002-000000000006"), new Guid("00000000-0000-0000-0001-000000000004") },
                    { new Guid("00000000-0000-0000-0002-000000000007"), new Guid("00000000-0000-0000-0001-000000000004") }
                });

            migrationBuilder.UpdateData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0020-000000000001") },
                column: "effect",
                value: 0);

            migrationBuilder.UpdateData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000002"), new Guid("00000000-0000-0000-0020-000000000001") },
                column: "effect",
                value: 0);

            migrationBuilder.UpdateData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000003"), new Guid("00000000-0000-0000-0020-000000000001") },
                column: "effect",
                value: 0);

            migrationBuilder.UpdateData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000004"), new Guid("00000000-0000-0000-0020-000000000001") },
                column: "effect",
                value: 0);

            migrationBuilder.UpdateData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000005"), new Guid("00000000-0000-0000-0020-000000000001") },
                column: "effect",
                value: 0);

            migrationBuilder.UpdateData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000006"), new Guid("00000000-0000-0000-0020-000000000001") },
                column: "effect",
                value: 0);

            migrationBuilder.UpdateData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000007"), new Guid("00000000-0000-0000-0020-000000000001") },
                column: "effect",
                value: 0);

            migrationBuilder.UpdateData(
                table: "user_permissions",
                keyColumns: new[] { "permission_id", "user_id" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0002-000000000001"), new Guid("00000000-0000-0000-0020-000000000002") },
                column: "effect",
                value: 0);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_expires_at",
                table: "user_sessions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_id",
                table: "user_sessions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropColumn(
                name: "effect",
                table: "user_permissions");
        }
    }
}
