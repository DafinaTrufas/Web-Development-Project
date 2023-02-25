using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrowdKnowledgeContribution.Data.Migrations
{
    public partial class ArticleHistoryDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArticleHistoryDate",
                table: "Comments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArticleHistoryId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ArticleHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProtectedArticleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleHistories", x => new { x.Id, x.Date });
                    table.ForeignKey(
                        name: "FK_ArticleHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArticleHistories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleHistories_ProtectedArticles_ProtectedArticleId",
                        column: x => x.ProtectedArticleId,
                        principalTable: "ProtectedArticles",
                        principalColumn: "ProtectedArticleId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ArticleHistoryId_ArticleHistoryDate",
                table: "Comments",
                columns: new[] { "ArticleHistoryId", "ArticleHistoryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleHistories_CategoryId",
                table: "ArticleHistories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleHistories_ProtectedArticleId",
                table: "ArticleHistories",
                column: "ProtectedArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleHistories_UserId",
                table: "ArticleHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_ArticleHistories_ArticleHistoryId_ArticleHistoryDate",
                table: "Comments",
                columns: new[] { "ArticleHistoryId", "ArticleHistoryDate" },
                principalTable: "ArticleHistories",
                principalColumns: new[] { "Id", "Date" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_ArticleHistories_ArticleHistoryId_ArticleHistoryDate",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "ArticleHistories");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ArticleHistoryId_ArticleHistoryDate",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ArticleHistoryDate",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ArticleHistoryId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Categories");
        }
    }
}
