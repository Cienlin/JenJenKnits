using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JenJenKnits.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Price = table.Column<int>(type: "INTEGER", nullable: true),
                    Material = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Dimensions = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ShortDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    BuyUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Featured = table.Column<int>(type: "INTEGER", nullable: true),
                    ColorSimulator = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    StoryMarkdown = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
