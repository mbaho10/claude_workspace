using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetPlanner.Infrastructure.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Departments", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(maxLength: 256, nullable: false),
                    FullName = table.Column<string>(maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(maxLength: 512, nullable: false),
                    Role = table.Column<byte>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    LastLoginAt = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey("FK_Users_Departments_DepartmentId", x => x.DepartmentId, "Departments", "Id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserDepartmentPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false),
                    PermissionLevel = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDepartmentPermissions", x => x.Id);
                    table.ForeignKey("FK_UserDeptPerm_Users", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_UserDeptPerm_Departments", x => x.DepartmentId, "Departments", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    Token = table.Column<string>(maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTime>(nullable: false),
                    RevokedAt = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey("FK_RefreshTokens_Users", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BudgetTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Year = table.Column<int>(nullable: false),
                    PeriodType = table.Column<byte>(nullable: false),
                    IsLocked = table.Column<bool>(nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetTemplates", x => x.Id);
                    table.ForeignKey("FK_BudgetTemplates_Users", x => x.CreatedBy, "Users", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BudgetCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    ParentId = table.Column<int>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                    IsCalculated = table.Column<bool>(nullable: false, defaultValue: false),
                    CategoryCode = table.Column<string>(maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCategories", x => x.Id);
                    table.ForeignKey("FK_BudgetCategories_Templates", x => x.TemplateId, "BudgetTemplates", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_BudgetCategories_Parent", x => x.ParentId, "BudgetCategories", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BudgetCells",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false),
                    PeriodIndex = table.Column<byte>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Note = table.Column<string>(maxLength: 500, nullable: true),
                    LastModifiedBy = table.Column<int>(nullable: false),
                    LastModifiedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCells", x => x.Id);
                    table.ForeignKey("FK_BudgetCells_Templates", x => x.TemplateId, "BudgetTemplates", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_BudgetCells_Categories", x => x.CategoryId, "BudgetCategories", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_BudgetCells_Departments", x => x.DepartmentId, "Departments", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_BudgetCells_Users", x => x.LastModifiedBy, "Users", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActionPlans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    IsLocked = table.Column<bool>(nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionPlans", x => x.Id);
                    table.ForeignKey("FK_ActionPlans_Templates", x => x.TemplateId, "BudgetTemplates", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_ActionPlans_Users", x => x.CreatedBy, "Users", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActionPlanItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    ActionPlanId = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<int>(nullable: false),
                    InitiativeName = table.Column<string>(maxLength: 300, nullable: false),
                    ResponsiblePerson = table.Column<string>(maxLength: 200, nullable: true),
                    StartDate = table.Column<DateOnly>(nullable: true),
                    EndDate = table.Column<DateOnly>(nullable: true),
                    BudgetAllocated = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BudgetSpent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<byte>(nullable: false, defaultValue: (byte)1),
                    Notes = table.Column<string>(maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                    LastModifiedBy = table.Column<int>(nullable: false),
                    LastModifiedAt = table.Column<DateTime>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionPlanItems", x => x.Id);
                    table.ForeignKey("FK_ActionPlanItems_ActionPlans", x => x.ActionPlanId, "ActionPlans", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ActionPlanItems_Departments", x => x.DepartmentId, "Departments", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_ActionPlanItems_Users", x => x.LastModifiedBy, "Users", "Id", onDelete: ReferentialAction.Restrict);
                });

            // Indexes
            migrationBuilder.CreateIndex("IX_Departments_Code", "Departments", "Code", unique: true);
            migrationBuilder.CreateIndex("IX_Users_Email", "Users", "Email", unique: true);
            migrationBuilder.CreateIndex("IX_UserDeptPerm_Unique", "UserDepartmentPermissions", new[] { "UserId", "DepartmentId" }, unique: true);
            migrationBuilder.CreateIndex("IX_RefreshTokens_Token", "RefreshTokens", "Token", unique: true);
            migrationBuilder.CreateIndex("IX_BudgetCells_Unique", "BudgetCells", new[] { "TemplateId", "CategoryId", "DepartmentId", "PeriodIndex" }, unique: true);

            // Seed data
            migrationBuilder.InsertData("Departments", new[] { "Id", "Name", "Code", "IsActive", "CreatedAt" }, new object[,]
            {
                { 1, "ฝ่ายบริหาร", "ADMIN", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                { 2, "ฝ่ายการเงิน", "FIN", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                { 3, "ฝ่ายทรัพยากรบุคคล", "HR", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                { 4, "ฝ่ายเทคโนโลยีสารสนเทศ", "IT", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                { 5, "ฝ่ายการตลาด", "MKT", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            });

            // Admin user is created at first startup in Program.cs with BCrypt hash
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("ActionPlanItems");
            migrationBuilder.DropTable("ActionPlans");
            migrationBuilder.DropTable("BudgetCells");
            migrationBuilder.DropTable("BudgetCategories");
            migrationBuilder.DropTable("BudgetTemplates");
            migrationBuilder.DropTable("RefreshTokens");
            migrationBuilder.DropTable("UserDepartmentPermissions");
            migrationBuilder.DropTable("Users");
            migrationBuilder.DropTable("Departments");
        }
    }
}
