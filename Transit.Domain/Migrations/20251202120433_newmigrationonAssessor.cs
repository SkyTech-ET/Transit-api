using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transit.Domain.Migrations
{
    /// <inheritdoc />
    public partial class newmigrationonAssessor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignmentNotes",
                table: "Services",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentNotes",
                table: "Services");
        }
    }
}
