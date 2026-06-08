using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<UserDepartmentPermission> UserDepartmentPermissions => Set<UserDepartmentPermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<BudgetTemplate> BudgetTemplates => Set<BudgetTemplate>();
    public DbSet<BudgetCategory> BudgetCategories => Set<BudgetCategory>();
    public DbSet<BudgetCell> BudgetCells => Set<BudgetCell>();
    public DbSet<ActionPlan> ActionPlans => Set<ActionPlan>();
    public DbSet<ActionPlanItem> ActionPlanItems => Set<ActionPlanItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            e.Property(x => x.Role).HasConversion<byte>();
            e.HasOne(x => x.Department).WithMany(d => d.Users).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserDepartmentPermission>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.DepartmentId }).IsUnique();
            e.Property(x => x.PermissionLevel).HasConversion<byte>();
            e.HasOne(x => x.User).WithMany(u => u.DepartmentPermissions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Department).WithMany(d => d.Permissions).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.Property(x => x.Token).HasMaxLength(512).IsRequired();
            e.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BudgetTemplate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.PeriodType).HasConversion<byte>();
            e.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BudgetCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CategoryCode).HasMaxLength(50);
            e.HasOne(x => x.Template).WithMany(t => t.Categories).HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Parent).WithMany(c => c.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BudgetCell>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TemplateId, x.CategoryId, x.DepartmentId, x.PeriodIndex }).IsUnique();
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Note).HasMaxLength(500);
            e.HasOne(x => x.Template).WithMany(t => t.Cells).HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(c => c.Cells).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Department).WithMany(d => d.BudgetCells).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LastModifiedByUser).WithMany().HasForeignKey(x => x.LastModifiedBy).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ActionPlan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasOne(x => x.Template).WithMany(t => t.ActionPlans).HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ActionPlanItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.InitiativeName).HasMaxLength(300).IsRequired();
            e.Property(x => x.ResponsiblePerson).HasMaxLength(200);
            e.Property(x => x.BudgetAllocated).HasColumnType("decimal(18,2)");
            e.Property(x => x.BudgetSpent).HasColumnType("decimal(18,2)");
            e.Property(x => x.Status).HasConversion<byte>();
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasOne(x => x.ActionPlan).WithMany(a => a.Items).HasForeignKey(x => x.ActionPlanId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Department).WithMany(d => d.ActionPlanItems).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.LastModifiedByUser).WithMany().HasForeignKey(x => x.LastModifiedBy).OnDelete(DeleteBehavior.Restrict);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "ฝ่ายบริหาร", Code = "ADMIN", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 2, Name = "ฝ่ายการเงิน", Code = "FIN", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 3, Name = "ฝ่ายทรัพยากรบุคคล", Code = "HR", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 4, Name = "ฝ่ายเทคโนโลยีสารสนเทศ", Code = "IT", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Department { Id = 5, Name = "ฝ่ายการตลาด", Code = "MKT", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Admin user is seeded in Program.cs at startup using BCrypt
    }
}
