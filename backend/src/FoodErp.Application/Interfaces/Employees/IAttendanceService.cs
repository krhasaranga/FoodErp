using FoodErp.Application.DTOs.Employees;

namespace FoodErp.Application.Interfaces.Employees;

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceDto>> GetByDateAsync(DateOnly date);
    Task<IEnumerable<AttendanceDto>> GetByEmployeeAsync(Guid employeeId, DateOnly from, DateOnly to);
    Task<IEnumerable<AttendanceSummaryDto>> GetMonthlySummaryAsync(int year, int month);
    Task<AttendanceDto> RecordAsync(RecordAttendanceRequest request);
    Task<AttendanceDto> UpdateAsync(Guid id, UpdateAttendanceRequest request);
    Task DeleteAsync(Guid id);
}
