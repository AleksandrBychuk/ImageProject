using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageProject.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_СonstituentСolor_UserImages_ImageId",
                table: "СonstituentСolor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_СonstituentСolor",
                table: "СonstituentСolor");

            migrationBuilder.RenameTable(
                name: "СonstituentСolor",
                newName: "СonstituentСolors");

            migrationBuilder.RenameIndex(
                name: "IX_СonstituentСolor_ImageId",
                table: "СonstituentСolors",
                newName: "IX_СonstituentСolors_ImageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_СonstituentСolors",
                table: "СonstituentСolors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_СonstituentСolors_UserImages_ImageId",
                table: "СonstituentСolors",
                column: "ImageId",
                principalTable: "UserImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_СonstituentСolors_UserImages_ImageId",
                table: "СonstituentСolors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_СonstituentСolors",
                table: "СonstituentСolors");

            migrationBuilder.RenameTable(
                name: "СonstituentСolors",
                newName: "СonstituentСolor");

            migrationBuilder.RenameIndex(
                name: "IX_СonstituentСolors_ImageId",
                table: "СonstituentСolor",
                newName: "IX_СonstituentСolor_ImageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_СonstituentСolor",
                table: "СonstituentСolor",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_СonstituentСolor_UserImages_ImageId",
                table: "СonstituentСolor",
                column: "ImageId",
                principalTable: "UserImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
