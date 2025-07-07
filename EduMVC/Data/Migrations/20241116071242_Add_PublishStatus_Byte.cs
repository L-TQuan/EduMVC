using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_PublishStatus_Byte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "PublishStatus",
                table: "Courses",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PublishStatus",
                table: "Courses",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }
    }
}
