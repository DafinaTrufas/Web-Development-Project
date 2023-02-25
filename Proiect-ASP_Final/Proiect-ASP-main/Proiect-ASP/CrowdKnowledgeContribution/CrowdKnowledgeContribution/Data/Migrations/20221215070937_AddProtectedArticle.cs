using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrowdKnowledgeContribution.Data.Migrations
{
    public partial class AddProtectedArticle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProtectedArticles",
                columns: table => new
                {
                    ProtectedArticleId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtectedArticles", x => x.ProtectedArticleId);
                    table.ForeignKey(
                        name: "FK_ProtectedArticles_Articles_ProtectedArticleId",
                        column: x => x.ProtectedArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtectedArticles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProtectedArticles_UserId",
                table: "ProtectedArticles",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProtectedArticles");
        }
    }
}
