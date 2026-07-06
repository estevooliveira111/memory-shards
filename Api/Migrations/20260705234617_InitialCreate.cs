using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemporaryMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    EncryptedContent = table.Column<string>(type: "TEXT", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Salt = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    IV = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemporaryMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryMessages_ExpiresAt",
                table: "TemporaryMessages",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_TemporaryMessages_Slug",
                table: "TemporaryMessages",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemporaryMessages");
        }
    }
}
