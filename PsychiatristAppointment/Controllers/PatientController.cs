using Dapper;
using Microsoft.AspNetCore.Mvc;
using PsychiatristAppointment.Context;
using PsychiatristAppointment.Models;

[Route("api/[controller]")]
[ApiController]
public class PatientsController : ControllerBase
{
    private readonly DapperContext _context;
    public PatientsController(DapperContext context) => _context = context;

    [HttpGet("Dashboard/{patientId}")]
    public async Task<IActionResult> GetDashboard(int patientId)
    {
        using var conn = _context.CreateConnection();
        
        // 1. Get Patient Details
        var patient = await conn.QueryFirstOrDefaultAsync<User>(@"SELECT Id, FullName, Email FROM Users WHERE Id = @Id", new { Id = patientId });
        if (patient == null) return NotFound("Patient not found");

        // 2. Get Upcoming Appointments
        var appointmentsQuery = @"
            SELECT a.AppointmentId, a.AppointmentDate, a.Status, a.Reason, a.SessionLink,
                   d.FullName AS DoctorName, d.Specialization
            FROM Appointments a
            INNER JOIN Users d ON a.DoctorId = d.Id AND d.Role = 'Doctor'
            WHERE a.PatientId = @Id AND a.AppointmentDate >= CAST(GETDATE() AS DATE)
            ORDER BY a.AppointmentDate";
            
        var appointments = await conn.QueryAsync<Appointment>(appointmentsQuery, new { Id = patientId });

        // 3. Get Recent Prescriptions
        var prescriptionsQuery = @"
            SELECT TOP 2 p.Id, p.PatientId, p.DoctorId, p.Diagnosis, p.Notes, p.Medicines, p.CreatedAt,
                   d.FullName AS DoctorName
            FROM Prescriptions p
            INNER JOIN Users d ON p.DoctorId = d.Id
            WHERE p.PatientId = @Id
            ORDER BY p.CreatedAt DESC";
        var prescriptions = await conn.QueryAsync<PrescriptionResponseDto>(prescriptionsQuery, new { Id = patientId });

        // 4. Get Top Recommendation
        var recommendation = await conn.QueryFirstOrDefaultAsync<DoctorProfileDto>(@"
            SELECT TOP 1 
                u.Id, u.FullName, u.Email, u.Specialization, u.Fee,
                (SELECT STRING_AGG(val, ', ') 
                 FROM (SELECT DISTINCT s.Day + ' (' + CONVERT(varchar(5), s.StartTime, 108) + '-' + CONVERT(varchar(5), s.EndTime, 108) + ')' AS val 
                       FROM Schedules s 
                       WHERE s.DoctorId = u.Id) AS t) AS AvailableDays
            FROM Users u
            WHERE u.Role = 'Doctor' AND u.IsActive = 1
            ORDER BY (SELECT COUNT(*) FROM Appointments a WHERE a.DoctorId = u.Id) DESC");

        return Ok(new PatientDashboardDto
        { 
            Patient = patient, 
            UpcomingAppointments = appointments,
            RecentPrescriptions = prescriptions,
            TopRecommendation = recommendation
        });
    }
}
