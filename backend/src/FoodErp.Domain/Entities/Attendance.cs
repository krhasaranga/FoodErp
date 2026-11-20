using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public enum AttendanceStatus
{
    Present,
    Absent,
    HalfDay,
    Leave,
    Holiday,
    WeekOff
}

/// <summary>
/// Daily attendance record for an employee.
/// One record per employee per date — enforced by a unique index.
/// </summary>
public class Attendance : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    public Guid EmployeeId { get; private set; }
    public Employee? Employee { get; private set; }

    public DateOnly Date { get; private set; }
    public AttendanceStatus Status { get; private set; }

    /// <summary>Time the employee checked in. Null if absent.</summary>
    public TimeOnly? CheckIn { get; private set; }

    /// <summary>Time the employee checked out. Null if absent or not yet checked out.</summary>
    public TimeOnly? CheckOut { get; private set; }

    public string? Notes { get; private set; }

    private Attendance() { }

    public static Attendance Create(
        Guid employeeId,
        DateOnly date,
        AttendanceStatus status,
        Guid tenantId,
        TimeOnly? checkIn = null,
        TimeOnly? checkOut = null,
        string? notes = null)
    {
        return new Attendance
        {
            EmployeeId = employeeId,
            Date       = date,
            Status     = status,
            TenantId   = tenantId,
            CheckIn    = checkIn,
            CheckOut   = checkOut,
            Notes      = notes?.Trim()
        };
    }

    public void Update(
        AttendanceStatus status,
        TimeOnly? checkIn,
        TimeOnly? checkOut,
        string? notes)
    {
        Status   = status;
        CheckIn  = checkIn;
        CheckOut = checkOut;
        Notes    = notes?.Trim();
        SetUpdatedAt();
    }
}
