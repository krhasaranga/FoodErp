using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Casual
}

/// <summary>
/// An employee belonging to a tenant.
/// Payroll is deferred — this record covers identity, role, and basic HR details.
/// </summary>
public class Employee : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>Short unique code, e.g. EMP-001.</summary>
    public string Code { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";

    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    /// <summary>e.g. "Production Supervisor", "Delivery Driver", "Accounts Clerk"</summary>
    public string? JobTitle { get; private set; }

    /// <summary>Free-text department name, e.g. "Production", "Logistics", "Finance".</summary>
    public string? Department { get; private set; }

    public EmploymentType EmploymentType { get; private set; }
    public DateTime DateOfJoining { get; private set; }

    /// <summary>Basic monthly or daily salary. Full payroll deferred.</summary>
    public decimal? BasicSalary { get; private set; }

    public bool IsActive { get; private set; } = true;

    public ICollection<Attendance> AttendanceRecords { get; private set; } = new List<Attendance>();

    private Employee() { }

    public static Employee Create(
        string code,
        string firstName,
        string lastName,
        EmploymentType employmentType,
        DateTime dateOfJoining,
        Guid tenantId,
        string? phone = null,
        string? email = null,
        string? jobTitle = null,
        string? department = null,
        decimal? basicSalary = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);

        return new Employee
        {
            Code           = code.Trim().ToUpperInvariant(),
            FirstName      = firstName.Trim(),
            LastName       = lastName.Trim(),
            EmploymentType = employmentType,
            DateOfJoining  = dateOfJoining.ToUniversalTime(),
            TenantId       = tenantId,
            Phone          = phone?.Trim(),
            Email          = email?.Trim().ToLowerInvariant(),
            JobTitle       = jobTitle?.Trim(),
            Department     = department?.Trim(),
            BasicSalary    = basicSalary
        };
    }

    public void Update(
        string firstName, string lastName,
        string? phone, string? email,
        string? jobTitle, string? department,
        EmploymentType employmentType,
        decimal? basicSalary)
    {
        FirstName      = firstName.Trim();
        LastName       = lastName.Trim();
        Phone          = phone?.Trim();
        Email          = email?.Trim().ToLowerInvariant();
        JobTitle       = jobTitle?.Trim();
        Department     = department?.Trim();
        EmploymentType = employmentType;
        BasicSalary    = basicSalary;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
