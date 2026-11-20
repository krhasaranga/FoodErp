using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.JobTitle).HasMaxLength(200);
        builder.Property(e => e.Department).HasMaxLength(100);
        builder.Property(e => e.EmploymentType).IsRequired();
        builder.Property(e => e.DateOfJoining).IsRequired();
        builder.Property(e => e.BasicSalary).HasColumnType("decimal(18,4)");
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        // Computed property — not stored
        builder.Ignore(e => e.FullName);

        builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.Department });

        builder.HasMany(e => e.AttendanceRecords)
               .WithOne(a => a.Employee)
               .HasForeignKey(a => a.EmployeeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
