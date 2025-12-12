using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transit.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddnewMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Users_CreatedByDataEncoderId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_CreatedByDataEncoderId",
                table: "Services");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ServiceStages",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "CreatedByUserId",
                table: "Services",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ServiceStages");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Services");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CreatedByDataEncoderId",
                table: "Services",
                column: "CreatedByDataEncoderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Users_CreatedByDataEncoderId",
                table: "Services",
                column: "CreatedByDataEncoderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
