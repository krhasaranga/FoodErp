using FoodErp.Application.DTOs.Employees;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Employees;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Employees;

public class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public AttendanceService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IEnumerable<AttendanceDto>> GetByDateAsync(DateOnly date)
    {
        var records = await _db.Attendance
            .Include(a => a.Employee)
            .Where(a => a.Date == date)
            .OrderBy(a => a.Employee!.LastName)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<IEnumerable<AttendanceDto>> GetByEmployeeAsync(
        Guid employeeId, DateOnly from, DateOnly to)
    {
        var records = await _db.Attendance
            .Include(a => a.Employee)
            .Where(a => a.EmployeeId == employeeId && a.Date >= from && a.Date <= to)
            .OrderBy(a => a.Date)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<IEnumerable<AttendanceSummaryDto>> GetMonthlySummaryAsync(int year, int month)
    {
        var from = new DateOnly(year, month, 1);
        var to   = from.AddMonths(1).AddDays(-1);

        var employees = await _db.Employees
            .Where(e => e.IsActive)
            .ToListAsync();

        var attendanceRecords = await _db.Attendance
            .Where(a => a.Date >= from && a.Date <= to)
            .ToListAsync();

        return employees.Select(e =>
        {
            var empRecords = attendanceRecords.Where(a => a.EmployeeId == e.Id).ToList();
            return new AttendanceSummaryDto(
                e.Id, e.Code,
                $"{e.FirstName} {e.LastName}",
                empRecords.Count(a => a.Status == AttendanceStatus.Present),
                empRecords.Count(a => a.Status == AttendanceStatus.Absent),
                empRecords.Count(a => a.Status == AttendanceStatus.HalfDay),
                empRecords.Count(a => a.Status == AttendanceStatus.Leave),
                empRecords.Count);
        }).OrderBy(s => s.EmployeeName);
    }

    public async Task<AttendanceDto> RecordAsync(RecordAttendanceRequest request)
    {
        // Check for duplicate: one record per employee per day
        var exists = await _db.Attendance.AnyAsync(
            a => a.EmployeeId == request.EmployeeId && a.Date == request.Date);

        if (exists)
            throw new InvalidOperationException(
                $"Attendance already recorded for this employee on {request.Date}. Use update instead.");

        var record = Attendance.Create(
            request.EmployeeId,
            request.Date,
            request.Status,
            _tenant.TenantId,
            request.CheckIn,
            request.CheckOut,
            request.Notes);

        _db.Attendance.Add(record);
        await _db.SaveChangesAsync();

        return await LoadDtoAsync(record.Id);
    }

    public async Task<AttendanceDto> UpdateAsync(Guid id, UpdateAttendanceRequest request)
    {
        var record = await _db.Attendance.FindAsync(id)
            ?? throw new KeyNotFoundException($"Attendance record {id} not found.");

        record.Update(request.Status, request.CheckIn, request.CheckOut, request.Notes);
        await _db.SaveChangesAsync();

        return await LoadDtoAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var record = await _db.Attendance.FindAsync(id)
            ?? throw new KeyNotFoundException($"Attendance record {id} not found.");

        _db.Attendance.Remove(record);
        await _db.SaveChangesAsync();
    }

    private async Task<AttendanceDto> LoadDtoAsync(Guid id)
    {
        var record = await _db.Attendance
            .Include(a => a.Employee)
            .FirstAsync(a => a.Id == id);
        return MapToDto(record);
    }

    private static AttendanceDto MapToDto(Attendance a) => new(
        a.Id, a.EmployeeId,
        a.Employee!.Code,
        $"{a.Employee.FirstName} {a.Employee.LastName}",
        a.Date, a.Status,
        a.CheckIn, a.CheckOut, a.Notes);
}
