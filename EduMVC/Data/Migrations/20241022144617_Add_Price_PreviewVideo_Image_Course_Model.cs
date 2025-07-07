using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Price_PreviewVideo_Image_Course_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviewMediumId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Courses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreviewMedia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreviewMedia", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ImageId",
                table: "Courses",
                column: "ImageId",
                unique: true,
                filter: "[ImageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_PreviewMediumId",
                table: "Courses",
                column: "PreviewMediumId",
                unique: true,
                filter: "[PreviewMediumId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Images_ImageId",
                table: "Courses",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_PreviewMedia_PreviewMediumId",
                table: "Courses",
                column: "PreviewMediumId",
                principalTable: "PreviewMedia",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Images_ImageId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_PreviewMedia_PreviewMediumId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "PreviewMedia");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ImageId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_PreviewMediumId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "PreviewMediumId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Courses");
        }
    }
}
