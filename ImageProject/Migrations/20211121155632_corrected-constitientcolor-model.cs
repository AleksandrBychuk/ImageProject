using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageProject.Migrations
{
    public partial class correctedconstitientcolormodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_СonstituentСolor_AspNetUsers_UserId",
                table: "СonstituentСolor");

            migrationBuilder.DropForeignKey(
                name: "FK_СonstituentСolor_UserImages_UserImageId",
                table: "СonstituentСolor");

            migrationBuilder.DropIndex(
                name: "IX_СonstituentСolor_UserId",
                table: "СonstituentСolor");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "СonstituentСolor");

            migrationBuilder.RenameColumn(
                name: "UserImageId",
                table: "СonstituentСolor",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_СonstituentСolor_UserImageId",
                table: "СonstituentСolor",
                newName: "IX_СonstituentСolor_ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_СonstituentСolor_UserImages_ImageId",
                table: "СonstituentСolor",
                column: "ImageId",
                principalTable: "UserImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_СonstituentСolor_UserImages_ImageId",
                table: "СonstituentСolor");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "СonstituentСolor",
                newName: "UserImageId");

            migrationBuilder.RenameIndex(
                name: "IX_СonstituentСolor_ImageId",
                table: "СonstituentСolor",
                newName: "IX_СonstituentСolor_UserImageId");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "СonstituentСolor",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_СonstituentСolor_UserId",
                table: "СonstituentСolor",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_СonstituentСolor_AspNetUsers_UserId",
                table: "СonstituentСolor",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_СonstituentСolor_UserImages_UserImageId",
                table: "СonstituentСolor",
                column: "UserImageId",
                principalTable: "UserImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
