using FoodErp.Application.DTOs.Employees;
using FoodErp.Application.Interfaces.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employees;
    private readonly IAttendanceService _attendance;

    public EmployeesController(IEmployeeService employees, IAttendanceService attendance)
    {
        _employees  = employees;
        _attendance = attendance;
    }

    // ── Employees ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? department)
        => Ok(await _employees.GetAllAsync(department));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _employees.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        var result = await _employees.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request)
        => Ok(await _employees.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _employees.DeactivateAsync(id);
        return NoContent();
    }

    // ── Attendance ─────────────────────────────────────────────────────────────

    [HttpGet("attendance/date/{date}")]
    public async Task<IActionResult> AttendanceByDate(DateOnly date)
        => Ok(await _attendance.GetByDateAsync(date));

    [HttpGet("{id:guid}/attendance")]
    public async Task<IActionResult> AttendanceByEmployee(
        Guid id,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
        => Ok(await _attendance.GetByEmployeeAsync(id, from, to));

    [HttpGet("attendance/summary")]
    public async Task<IActionResult> MonthlySummary(
        [FromQuery] int year,
        [FromQuery] int month)
        => Ok(await _attendance.GetMonthlySummaryAsync(year, month));

    [HttpPost("attendance")]
    public async Task<IActionResult> RecordAttendance([FromBody] RecordAttendanceRequest request)
        => Ok(await _attendance.RecordAsync(request));

    [HttpPut("attendance/{id:guid}")]
    public async Task<IActionResult> UpdateAttendance(Guid id, [FromBody] UpdateAttendanceRequest request)
        => Ok(await _attendance.UpdateAsync(id, request));

    [HttpDelete("attendance/{id:guid}")]
    public async Task<IActionResult> DeleteAttendance(Guid id)
    {
        await _attendance.DeleteAsync(id);
        return NoContent();
    }
}
