using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UC_Web_Assessment.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLikeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "AIImage",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ImageLike",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AIImageId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LikedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageLike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageLike_AIImage_AIImageId",
                        column: x => x.AIImageId,
                        principalTable: "AIImage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageLike_AIImageId_UserId",
                table: "ImageLike",
                columns: new[] { "AIImageId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageLike");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "AIImage");
        }
    }
}
