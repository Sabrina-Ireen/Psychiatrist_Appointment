using Dapper;
using Microsoft.AspNetCore.Mvc;
using PsychiatristAppointment.Context;
using PsychiatristAppointment.Models;
using System;
using System;

[Route("api/[controller]")]
[ApiController]
public class DoctorsController : ControllerBase
{
    private readonly DapperContext _context;

    public DoctorsController(DapperContext context)
    {
        _context = context;
    }

    [HttpGet("Dashboard/{doctorId}")]
    public async Task<IActionResult> GetDoctorDashboard(int doctorId)
    {
        var query = @"
SELECT
    a.PatientId,
    u.FullName AS PatientName,
    a.AppointmentDate,
    CONVERT(varchar(5), a.SlotTime, 108) AS SlotTime,
    a.SessionLink
FROM Appointments a
INNER JOIN Users u ON a.PatientId = u.Id
WHERE a.DoctorId = @DoctorId
  AND a.AppointmentDate >= CAST(GETDATE() AS DATE)
ORDER BY a.AppointmentDate, a.SlotTime";

        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<DoctorSlotDto>(query, new { DoctorId = doctorId });
        return Ok(result);

    }
    [HttpGet("Sessions/{doctorId}")]
    public async Task<IActionResult> GetDoctorSessions(int doctorId)
    {
        var query = @"
SELECT
    a.AppointmentId,
    u.FullName AS PatientName,
    CONVERT(varchar, a.AppointmentDate, 23) AS AppointmentDate,
    CAST(a.SlotTime AS VARCHAR(8)) AS SlotTime,
    p.Diagnosis,
    p.Notes,
    p.Medicines,
    a.SessionLink
FROM Appointments a
INNER JOIN Users u ON a.PatientId = u.Id
INNER JOIN Prescriptions p ON a.AppointmentId = p.AppointmentId
WHERE a.DoctorId = @DoctorId
ORDER BY a.AppointmentDate DESC, a.SlotTime
";

        using var connection = _context.CreateConnection();
        var result = await connection.QueryAsync<DoctorSessionDto>(query, new { DoctorId = doctorId });

        return Ok(result);
    }

    [HttpGet("Profile/{doctorId}")]
    public async Task<IActionResult> GetDoctorProfile(int doctorId)
    {
        var query = @"
        SELECT 
            Id,
            FullName,
            Email,
            Specialization,
            IsActive
        FROM Users
        WHERE Id = @Id AND Role = 'Doctor'";

        using var connection = _context.CreateConnection();

        var doctor = await connection.QueryFirstOrDefaultAsync<DoctorProfileDto>(
            query, new { Id = doctorId });

        if (doctor == null)
            return NotFound("Doctor not found");

        return Ok(doctor);
    }
    [HttpPut("Profile/{doctorId}")]
    public async Task<IActionResult> UpdateDoctorProfile(int doctorId, [FromBody] DoctorProfileDto dto)
    {
        var query = @"
        UPDATE Users SET
            FullName = @FullName,
            Specialization = @Specialization,
            IsActive = @IsActive
        WHERE Id = @Id AND Role = 'Doctor'";

        using var connection = _context.CreateConnection();

        await connection.ExecuteAsync(query, new
        {
            Id = doctorId,
            dto.FullName,
            dto.Specialization,
            dto.IsActive  // Dapper will convert bool -> BIT
        });

        return Ok(new { message = "Doctor profile updated successfully" });
    }

    [HttpGet("Patients/{doctorId}")]
    public async Task<IActionResult> GetDoctorPatients(int doctorId)
    {
        var query = @"
        SELECT DISTINCT u.Id AS PatientId, u.FullName AS PatientName
        FROM Appointments a
        INNER JOIN Users u ON a.PatientId = u.Id
        WHERE a.DoctorId = @DoctorId
        ORDER BY u.FullName
    ";

        using var connection = _context.CreateConnection();
        var patients = await connection.QueryAsync<DoctorPatientDto>(
            query,
            new { DoctorId = doctorId }
        );

        return Ok(patients);
    }

    [HttpGet("Patients/Today/{doctorId}")]
    public async Task<IActionResult> GetDoctorPatientsToday(int doctorId)
    {
        var query = @"
        SELECT DISTINCT u.Id AS PatientId, u.FullName AS PatientName
        FROM Appointments a
        INNER JOIN Users u ON a.PatientId = u.Id
        WHERE a.DoctorId = @DoctorId
          AND CAST(a.AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
        ORDER BY u.FullName
    ";

        using var connection = _context.CreateConnection();
        var patients = await connection.QueryAsync<DoctorPatientDto>(
            query,
            new { DoctorId = doctorId }
        );

        return Ok(patients);
    }

    [HttpGet("UpcomingPatients/{doctorId}")]
    public async Task<IActionResult> GetDoctorPatientsUpcoming(int doctorId)
    {
        var query = @"
        SELECT DISTINCT u.Id AS PatientId, u.FullName AS PatientName
        FROM Appointments a
        INNER JOIN Users u ON a.PatientId = u.Id
        WHERE a.DoctorId = @DoctorId
          AND CAST(a.AppointmentDate AS DATE) >= CAST(GETDATE() AS DATE)
        ORDER BY u.FullName
    ";

        using var connection = _context.CreateConnection();
        var patients = await connection.QueryAsync<DoctorPatientDto>(
            query,
            new { DoctorId = doctorId }
        );

        return Ok(patients);
    }

    [HttpGet("All")]  // GET: api/Doctors/All
    public async Task<IActionResult> GetAllDoctors()
    {
        var query = @"
SELECT 
    u.FullName,
    u.Id,
    u.Specialization,
    u.Email,
    u.IsActive,
    u.Fee,
    (SELECT STRING_AGG(val, ', ') 
     FROM (SELECT DISTINCT s.Day + ' (' + CONVERT(varchar(5), s.StartTime, 108) + '-' + CONVERT(varchar(5), s.EndTime, 108) + ')' AS val 
           FROM Schedules s 
           WHERE s.DoctorId = u.Id) AS t) AS AvailableDays
FROM Users u
WHERE u.Role = 'Doctor' AND u.IsActive = 1
ORDER BY u.FullName";

        using var connection = _context.CreateConnection();
        var doctors = await connection.QueryAsync<DoctorProfileDto>(query);

        return Ok(doctors);
    }
    [HttpGet("Schedule")]
    public async Task<IActionResult> GetDoctorSchedule(int doctorId, string day)
    {
        var query = @"
        SELECT DoctorId, Day, 
               CONVERT(varchar(5), StartTime, 108) AS StartTime, 
               CONVERT(varchar(5), EndTime, 108) AS EndTime
        FROM Schedules
        WHERE DoctorId = @DoctorId AND Day = @Day
    ";

        using var connection = _context.CreateConnection();
        var schedule = await connection.QueryFirstOrDefaultAsync<Schedule>(
            query,
            new { DoctorId = doctorId, Day = day }
        );

        if (schedule == null)
            return Ok(new List<string>());

        return Ok(schedule);
    }

    [HttpGet("AvailableSlots")]
    public async Task<IActionResult> GetAvailableSlots(int doctorId, string date)
    {
        if (!DateTime.TryParse(date, out var appointmentDate))
            return BadRequest("Invalid date");

        var dayOfWeek = appointmentDate.DayOfWeek.ToString();

        using var connection = _context.CreateConnection();

        // Get doctor schedule with TIME → string conversion
        var scheduleQuery = @"
    SELECT 
        DoctorId,
        Day,
        CONVERT(varchar(5), StartTime, 108) AS StartTime,
        CONVERT(varchar(5), EndTime, 108) AS EndTime
    FROM Schedules 
    WHERE DoctorId=@DoctorId AND Day=@Day";

        var schedule = await connection.QueryFirstOrDefaultAsync<Schedule>(
            scheduleQuery,
            new { DoctorId = doctorId, Day = dayOfWeek }
        );

        if (schedule == null)
            return Ok(new List<string>());

        // Get already booked slots
        var bookedQuery = @"SELECT SlotTime FROM Appointments 
                        WHERE DoctorId=@DoctorId AND AppointmentDate=@Date";
        var bookedSlots = (await connection.QueryAsync<string>(
            bookedQuery,
            new { DoctorId = doctorId, Date = appointmentDate.Date }
        )).ToList();

        // Generate slots (hourly)
        var availableSlots = new List<string>();
        var start = TimeSpan.Parse(schedule.StartTime);
        var end = TimeSpan.Parse(schedule.EndTime);

        while (start < end)
        {
            var slotStr = start.ToString(@"hh\:mm");
            if (!bookedSlots.Contains(slotStr))
                availableSlots.Add(slotStr);

            start = start.Add(TimeSpan.FromHours(1)); // 1-hour slot
        }

        return Ok(availableSlots);
    }



    // 9️⃣ Book appointment
    [HttpPost("Book")]
    public async Task<IActionResult> BookAppointment([FromBody] Appointment appointment)
    {
        var query = @"
INSERT INTO Appointments (DoctorId, PatientId, SlotTime, AppointmentDate, Status)
VALUES (@DoctorId, @PatientId, @SlotTime, @AppointmentDate, @Status);
SELECT CAST(SCOPE_IDENTITY() as int);";

        using var conn = _context.CreateConnection();
        var id = await conn.ExecuteScalarAsync<int>(query, new
        {
            appointment.DoctorId,
            appointment.PatientId,
            appointment.SlotTime,
            appointment.AppointmentDate,
            Status = "Pending"
        });

        return Ok(new { AppointmentId = id, Message = "Appointment booked successfully" });
    }
}
