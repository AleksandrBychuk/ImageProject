using Microsoft.EntityFrameworkCore.Migrations;

namespace ImageProject.Migrations
{
    public partial class integrationImageCoord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageCoords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageId = table.Column<int>(type: "int", nullable: false),
                    LatitudeDegree = table.Column<float>(type: "real", nullable: false),
                    LatitudeMinute = table.Column<float>(type: "real", nullable: false),
                    LatitudeSecond = table.Column<float>(type: "real", nullable: true),
                    LongitudeDegree = table.Column<float>(type: "real", nullable: false),
                    LongitudeMinute = table.Column<float>(type: "real", nullable: false),
                    LongitudeSecond = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageCoords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageCoords_UserImages_ImageId",
                        column: x => x.ImageId,
                        principalTable: "UserImages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageCoords_ImageId",
                table: "ImageCoords",
                column: "ImageId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageCoords");
        }
    }
}
