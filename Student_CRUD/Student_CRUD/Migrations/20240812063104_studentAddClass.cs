using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Student_CRUD.Migrations
{
    /// <inheritdoc />
    public partial class studentAddClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Standart",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Standart",
                table: "Students");
        }
    }
}
