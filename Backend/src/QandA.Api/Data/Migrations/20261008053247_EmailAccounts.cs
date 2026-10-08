using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace QandA.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class EmailAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-written instead of the scaffolded drop/add, so existing accounts keep their data:
            // the old login becomes the email and, for now, also the display name.
            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedLogin",
                table: "Users");

            migrationBuilder.RenameColumn(name: "Login", table: "Users", newName: "Email");
            migrationBuilder.RenameColumn(name: "NormalizedLogin", table: "Users", newName: "NormalizedEmail");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailConfirmedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "Users",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "uk");

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Users" SET
                    "DisplayName" = left("Email", 50),
                    "SecurityStamp" = md5(random()::text || "Id"::text);
                """);

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_TokenHash",
                table: "UserTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId_Purpose",
                table: "UserTokens",
                columns: new[] { "UserId", "Purpose" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UserTokens");
            migrationBuilder.DropIndex(name: "IX_Users_NormalizedEmail", table: "Users");
            migrationBuilder.DropColumn(name: "DisplayName", table: "Users");
            migrationBuilder.DropColumn(name: "EmailConfirmedAt", table: "Users");
            migrationBuilder.DropColumn(name: "Locale", table: "Users");
            migrationBuilder.DropColumn(name: "SecurityStamp", table: "Users");
            migrationBuilder.RenameColumn(name: "Email", table: "Users", newName: "Login");
            migrationBuilder.RenameColumn(name: "NormalizedEmail", table: "Users", newName: "NormalizedLogin");
            migrationBuilder.Sql("""UPDATE "Users" SET "Login" = left("Login", 30), "NormalizedLogin" = left("NormalizedLogin", 30);""");
            migrationBuilder.AlterColumn<string>(name: "Login", table: "Users", type: "character varying(30)", maxLength: 30, nullable: false);
            migrationBuilder.AlterColumn<string>(name: "NormalizedLogin", table: "Users", type: "character varying(30)", maxLength: 30, nullable: false);
            migrationBuilder.CreateIndex(name: "IX_Users_NormalizedLogin", table: "Users", column: "NormalizedLogin", unique: true);
        }
    }
}
