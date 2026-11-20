using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("Attendance");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId).IsRequired();
        builder.Property(a => a.EmployeeId).IsRequired();
        builder.Property(a => a.Date).IsRequired();
        builder.Property(a => a.Status).IsRequired();
        builder.Property(a => a.Notes).HasMaxLength(500);
        builder.Property(a => a.CreatedAt).IsRequired();

        // One attendance record per employee per day
        builder.HasIndex(a => new { a.TenantId, a.EmployeeId, a.Date }).IsUnique();
        builder.HasIndex(a => new { a.TenantId, a.Date });
    }
}
