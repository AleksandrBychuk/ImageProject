using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageProject.Migrations
{
    public partial class added_new_model_constituentcolor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "СonstituentСolor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HexColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserImageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_СonstituentСolor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_СonstituentСolor_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_СonstituentСolor_UserImages_UserImageId",
                        column: x => x.UserImageId,
                        principalTable: "UserImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_СonstituentСolor_UserId",
                table: "СonstituentСolor",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_СonstituentСolor_UserImageId",
                table: "СonstituentСolor",
                column: "UserImageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "СonstituentСolor");
        }
    }
}
