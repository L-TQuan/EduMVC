using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class PublishStatus_CreatedDate_Fields_Course_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CreatedDate",
                table: "Courses",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "PublishStatus",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "Courses");
        }
    }
}
