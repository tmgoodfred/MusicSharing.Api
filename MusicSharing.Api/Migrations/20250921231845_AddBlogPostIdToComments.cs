using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicSharing.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPostIdToComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Songs_SongId",
                schema: "MusicSharing",
                table: "Comments");

            migrationBuilder.AlterColumn<int>(
                name: "SongId",
                schema: "MusicSharing",
                table: "Comments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "BlogPostId",
                schema: "MusicSharing",
                table: "Comments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BlogPostId",
                schema: "MusicSharing",
                table: "Comments",
                column: "BlogPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_BlogPosts_BlogPostId",
                schema: "MusicSharing",
                table: "Comments",
                column: "BlogPostId",
                principalSchema: "MusicSharing",
                principalTable: "BlogPosts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Songs_SongId",
                schema: "MusicSharing",
                table: "Comments",
                column: "SongId",
                principalSchema: "MusicSharing",
                principalTable: "Songs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_BlogPosts_BlogPostId",
                schema: "MusicSharing",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Songs_SongId",
                schema: "MusicSharing",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_BlogPostId",
                schema: "MusicSharing",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "BlogPostId",
                schema: "MusicSharing",
                table: "Comments");

            migrationBuilder.AlterColumn<int>(
                name: "SongId",
                schema: "MusicSharing",
                table: "Comments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Songs_SongId",
                schema: "MusicSharing",
                table: "Comments",
                column: "SongId",
                principalSchema: "MusicSharing",
                principalTable: "Songs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
