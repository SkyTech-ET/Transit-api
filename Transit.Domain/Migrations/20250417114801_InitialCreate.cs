using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transit.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Privilege",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeZoneInfo = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RegisteredBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    RecordStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privilege", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeZoneInfo = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RegisteredBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    RecordStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAccountLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerificationToken = table.Column<string>(type: "TEXT", nullable: false),
                    LoginAttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastLoginDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsConfirmationEmailSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    userTokenLifetime = table.Column<int>(type: "INTEGER", nullable: false),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    RefreshTokenExpireDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeZoneInfo = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RegisteredBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    RecordStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePrivilege",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: false),
                    PrivilegeId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeZoneInfo = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RegisteredBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    RecordStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePrivilege", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePrivilege_Privilege_PrivilegeId",
                        column: x => x.PrivilegeId,
                        principalTable: "Privilege",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePrivilege_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: false),
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeZoneInfo = table.Column<string>(type: "TEXT", nullable: false),
                    RegisteredDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RegisteredBy = table.Column<string>(type: "TEXT", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    RecordStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRole_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePrivilege_PrivilegeId",
                table: "RolePrivilege",
                column: "PrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePrivilege_RoleId",
                table: "RolePrivilege",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePrivilege");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Privilege");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
