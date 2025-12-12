using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transit.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddMyfirstMygration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceMessages_Users_RecipientUserId",
                table: "ServiceMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceMessages_Users_SenderUserId",
                table: "ServiceMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_StageComments_Users_CommentedByUserId",
                table: "StageComments");

            migrationBuilder.DropForeignKey(
                name: "FK_StageDocuments_Users_UploadedByUserId",
                table: "StageDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_StageDocuments_Users_VerifiedByUserId",
                table: "StageDocuments");

            migrationBuilder.DropColumn(
                name: "CommentType",
                table: "StageComments");

            migrationBuilder.DropColumn(
                name: "IsInternal",
                table: "StageComments");

            migrationBuilder.DropColumn(
                name: "IsVisibleToCustomer",
                table: "StageComments");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceMessages_Users_RecipientUserId",
                table: "ServiceMessages",
                column: "RecipientUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceMessages_Users_SenderUserId",
                table: "ServiceMessages",
                column: "SenderUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StageComments_Users_CommentedByUserId",
                table: "StageComments",
                column: "CommentedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StageDocuments_Users_UploadedByUserId",
                table: "StageDocuments",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StageDocuments_Users_VerifiedByUserId",
                table: "StageDocuments",
                column: "VerifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceMessages_Users_RecipientUserId",
                table: "ServiceMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceMessages_Users_SenderUserId",
                table: "ServiceMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_StageComments_Users_CommentedByUserId",
                table: "StageComments");

            migrationBuilder.DropForeignKey(
                name: "FK_StageDocuments_Users_UploadedByUserId",
                table: "StageDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_StageDocuments_Users_VerifiedByUserId",
                table: "StageDocuments");

            migrationBuilder.AddColumn<string>(
                name: "CommentType",
                table: "StageComments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInternal",
                table: "StageComments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisibleToCustomer",
                table: "StageComments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceMessages_Users_RecipientUserId",
                table: "ServiceMessages",
                column: "RecipientUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceMessages_Users_SenderUserId",
                table: "ServiceMessages",
                column: "SenderUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StageComments_Users_CommentedByUserId",
                table: "StageComments",
                column: "CommentedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StageDocuments_Users_UploadedByUserId",
                table: "StageDocuments",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StageDocuments_Users_VerifiedByUserId",
                table: "StageDocuments",
                column: "VerifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
