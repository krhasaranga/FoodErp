using FoodErp.Domain.Entities;

namespace FoodErp.Application.DTOs.Employees;

// ── Employee ──────────────────────────────────────────────────────────────────

public record EmployeeDto(
    Guid Id,
    string Code,
    string FirstName,
    string LastName,
    string FullName,
    string? Phone,
    string? Email,
    string? JobTitle,
    string? Department,
    EmploymentType EmploymentType,
    DateTime DateOfJoining,
    decimal? BasicSalary,
    bool IsActive,
    DateTime CreatedAt);

public record EmployeeListDto(
    Guid Id,
    string Code,
    string FullName,
    string? JobTitle,
    string? Department,
    EmploymentType EmploymentType,
    bool IsActive);

public record CreateEmployeeRequest(
    string Code,
    string FirstName,
    string LastName,
    EmploymentType EmploymentType,
    DateTime DateOfJoining,
    string? Phone,
    string? Email,
    string? JobTitle,
    string? Department,
    decimal? BasicSalary);

public record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string? Email,
    string? JobTitle,
    string? Department,
    EmploymentType EmploymentType,
    decimal? BasicSalary);

// ── Attendance ────────────────────────────────────────────────────────────────

public record AttendanceDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    DateOnly Date,
    AttendanceStatus Status,
    TimeOnly? CheckIn,
    TimeOnly? CheckOut,
    string? Notes);

public record RecordAttendanceRequest(
    Guid EmployeeId,
    DateOnly Date,
    AttendanceStatus Status,
    TimeOnly? CheckIn,
    TimeOnly? CheckOut,
    string? Notes);

public record UpdateAttendanceRequest(
    AttendanceStatus Status,
    TimeOnly? CheckIn,
    TimeOnly? CheckOut,
    string? Notes);

public record AttendanceSummaryDto(
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    int PresentDays,
    int AbsentDays,
    int HalfDays,
    int LeaveDays,
    int TotalRecorded);
