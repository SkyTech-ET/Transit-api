using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transit.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddMOTSystemEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessName = table.Column<string>(type: "TEXT", nullable: false),
                    TINNumber = table.Column<string>(type: "TEXT", nullable: false),
                    BusinessLicense = table.Column<string>(type: "TEXT", nullable: false),
                    BusinessAddress = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", nullable: false),
                    ContactPhone = table.Column<string>(type: "TEXT", nullable: false),
                    ContactEmail = table.Column<string>(type: "TEXT", nullable: false),
                    BusinessType = table.Column<string>(type: "TEXT", nullable: false),
                    ImportLicense = table.Column<string>(type: "TEXT", nullable: false),
                    ImportLicenseExpiry = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VerifiedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    VerificationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByDataEncoderId = table.Column<long>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Users_CreatedByDataEncoderId",
                        column: x => x.CreatedByDataEncoderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServiceNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ItemDescription = table.Column<string>(type: "TEXT", nullable: false),
                    RouteCategory = table.Column<string>(type: "TEXT", nullable: false),
                    DeclaredValue = table.Column<decimal>(type: "TEXT", nullable: false),
                    TaxCategory = table.Column<string>(type: "TEXT", nullable: false),
                    CountryOfOrigin = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceType = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RiskLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomerId = table.Column<long>(type: "INTEGER", nullable: false),
                    AssignedCaseExecutorId = table.Column<long>(type: "INTEGER", nullable: true),
                    AssignedAssessorId = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedByDataEncoderId = table.Column<long>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Users_AssignedAssessorId",
                        column: x => x.AssignedAssessorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Services_Users_AssignedCaseExecutorId",
                        column: x => x.AssignedCaseExecutorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Services_Users_CreatedByDataEncoderId",
                        column: x => x.CreatedByDataEncoderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Services_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileExtension = table.Column<string>(type: "TEXT", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentType = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerificationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedByUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    VerifiedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
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
                    table.PrimaryKey("PK_CustomerDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDocuments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerDocuments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerDocuments_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceStages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Stage = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Comments = table.Column<string>(type: "TEXT", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssignedTo = table.Column<string>(type: "TEXT", nullable: true),
                    RiskNotes = table.Column<string>(type: "TEXT", nullable: true),
                    RequiresCustomerAction = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBlocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    BlockReason = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceId = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
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
                    table.PrimaryKey("PK_ServiceStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceStages_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceStages_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    SentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActionUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ActionText = table.Column<string>(type: "TEXT", nullable: true),
                    IsUrgent = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    ServiceId = table.Column<long>(type: "INTEGER", nullable: true),
                    ServiceStageId = table.Column<long>(type: "INTEGER", nullable: true),
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
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_ServiceStages_ServiceStageId",
                        column: x => x.ServiceStageId,
                        principalTable: "ServiceStages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notifications_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileExtension = table.Column<string>(type: "TEXT", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentType = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerificationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceId = table.Column<long>(type: "INTEGER", nullable: false),
                    ServiceStageId = table.Column<long>(type: "INTEGER", nullable: true),
                    UploadedByUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    VerifiedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
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
                    table.PrimaryKey("PK_ServiceDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceDocuments_ServiceStages_ServiceStageId",
                        column: x => x.ServiceStageId,
                        principalTable: "ServiceStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceDocuments_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceDocuments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceDocuments_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ServiceMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    MessageType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsUrgent = table.Column<bool>(type: "INTEGER", nullable: false),
                    Priority = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceId = table.Column<long>(type: "INTEGER", nullable: false),
                    SenderUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    RecipientUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    ServiceStageId = table.Column<long>(type: "INTEGER", nullable: true),
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
                    table.PrimaryKey("PK_ServiceMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceMessages_ServiceStages_ServiceStageId",
                        column: x => x.ServiceStageId,
                        principalTable: "ServiceStages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceMessages_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceMessages_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Comment = table.Column<string>(type: "TEXT", nullable: false),
                    CommentType = table.Column<string>(type: "TEXT", nullable: true),
                    IsInternal = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVisibleToCustomer = table.Column<bool>(type: "INTEGER", nullable: false),
                    ServiceStageId = table.Column<long>(type: "INTEGER", nullable: false),
                    CommentedByUserId = table.Column<long>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_StageComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageComments_ServiceStages_ServiceStageId",
                        column: x => x.ServiceStageId,
                        principalTable: "ServiceStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageComments_Users_CommentedByUserId",
                        column: x => x.CommentedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileExtension = table.Column<string>(type: "TEXT", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentType = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerificationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceStageId = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedByUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    VerifiedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
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
                    table.PrimaryKey("PK_StageDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageDocuments_ServiceStages_ServiceStageId",
                        column: x => x.ServiceStageId,
                        principalTable: "ServiceStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageDocuments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageDocuments_Users_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocuments_CustomerId",
                table: "CustomerDocuments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocuments_UploadedByUserId",
                table: "CustomerDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDocuments_VerifiedByUserId",
                table: "CustomerDocuments",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedByDataEncoderId",
                table: "Customers",
                column: "CreatedByDataEncoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_VerifiedByUserId",
                table: "Customers",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ServiceId",
                table: "Notifications",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ServiceStageId",
                table: "Notifications",
                column: "ServiceStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDocuments_ServiceId",
                table: "ServiceDocuments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDocuments_ServiceStageId",
                table: "ServiceDocuments",
                column: "ServiceStageId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDocuments_UploadedByUserId",
                table: "ServiceDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDocuments_VerifiedByUserId",
                table: "ServiceDocuments",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMessages_RecipientUserId",
                table: "ServiceMessages",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMessages_SenderUserId",
                table: "ServiceMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMessages_ServiceId",
                table: "ServiceMessages",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMessages_ServiceStageId",
                table: "ServiceMessages",
                column: "ServiceStageId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_AssignedAssessorId",
                table: "Services",
                column: "AssignedAssessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_AssignedCaseExecutorId",
                table: "Services",
                column: "AssignedCaseExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CreatedByDataEncoderId",
                table: "Services",
                column: "CreatedByDataEncoderId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CustomerId",
                table: "Services",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceStages_ServiceId",
                table: "ServiceStages",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceStages_UpdatedByUserId",
                table: "ServiceStages",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StageComments_CommentedByUserId",
                table: "StageComments",
                column: "CommentedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StageComments_ServiceStageId",
                table: "StageComments",
                column: "ServiceStageId");

            migrationBuilder.CreateIndex(
                name: "IX_StageDocuments_ServiceStageId",
                table: "StageDocuments",
                column: "ServiceStageId");

            migrationBuilder.CreateIndex(
                name: "IX_StageDocuments_UploadedByUserId",
                table: "StageDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StageDocuments_VerifiedByUserId",
                table: "StageDocuments",
                column: "VerifiedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDocuments");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ServiceDocuments");

            migrationBuilder.DropTable(
                name: "ServiceMessages");

            migrationBuilder.DropTable(
                name: "StageComments");

            migrationBuilder.DropTable(
                name: "StageDocuments");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "ServiceStages");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
