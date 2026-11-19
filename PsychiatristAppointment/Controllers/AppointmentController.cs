using Dapper;
using PsychiatristAppointment.Models;
using Microsoft.AspNetCore.Mvc;
using PsychiatristAppointment.Context;

namespace PsychiatristAppointment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly DapperContext _context;
        public AppointmentsController(DapperContext context) => _context = context;

        [HttpGet("AvailableSlots/{doctorId}/{date}")]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, string date)
        {
            using var conn = _context.CreateConnection();

            // 1️⃣ Convert string to DateTime for the date
            if (!DateTime.TryParse(date, out DateTime selectedDate))
                return BadRequest("Invalid date format. Use yyyy-MM-dd.");

            string dayOfWeek = selectedDate.DayOfWeek.ToString(); // e.g., "Monday"

            // 2️⃣ Get doctor's schedule for that day
            var scheduleQuery = @"SELECT StartTime, EndTime
                          FROM Schedules
                          WHERE DoctorId=@DoctorId AND Day=@Day";
            var schedule = await conn.QueryFirstOrDefaultAsync(scheduleQuery, new { DoctorId = doctorId, Day = dayOfWeek });

            if (schedule == null)
                return Ok(new List<string>()); // no schedule = no slots

            // 3️⃣ Generate slots as strings
            // Assume StartTime and EndTime are strings like "09:00" or "14:00"
            string startStr = schedule.StartTime.ToString(); // e.g., "09:00"
            string endStr = schedule.EndTime.ToString();     // e.g., "17:00"

            if (!TimeSpan.TryParse(startStr, out TimeSpan start))
                return BadRequest("Invalid StartTime format in database");

            if (!TimeSpan.TryParse(endStr, out TimeSpan end))
                return BadRequest("Invalid EndTime format in database");

            List<string> slots = new List<string>();
            while (start < end)
            {
                slots.Add(start.ToString(@"hh\:mm")); // always as string
                start = start.Add(TimeSpan.FromHours(1)); // 1-hour slots
            }

            // 4️⃣ Remove already booked slots
            var bookedQuery = @"SELECT SlotTime
                        FROM Appointments
                        WHERE DoctorId=@DoctorId AND CAST(AppointmentDate AS DATE)=@Date";
            var bookedSlots = (await conn.QueryAsync<string>(bookedQuery, new { DoctorId = doctorId, Date = selectedDate.Date })).ToList();

            var availableSlots = slots.Except(bookedSlots).ToList();

            return Ok(availableSlots); // slots as strings
        }


        // Create a new appointment
        [HttpPost("Book")]
        public async Task<IActionResult> BookAppointment([FromBody] Appointment dto)
        {
            var query = @"
        INSERT INTO Appointments 
        (DoctorId, PatientId, SlotTime, AppointmentDate, Fee, PaidStatus, Status, Mobile, Age, Gender, Notes)
        VALUES 
        (@DoctorId, @PatientId, @SlotTime, @AppointmentDate, @Fee, 'Unpaid', 'Pending', @Mobile, @Age, @Gender, @Notes);
        
        SELECT CAST(SCOPE_IDENTITY() as int);
    ";

            using var conn = _context.CreateConnection();
            var appointmentId = await conn.ExecuteScalarAsync<int>(query, dto);

            return Ok(new { appointmentId });
        }


        [HttpGet("Patient/{patientId}")]
        public async Task<IActionResult> GetAppointmentsByPatient(int patientId)
        {
            var query = @"
                SELECT a.AppointmentId, a.DoctorId, a.PatientId, a.SlotTime, a.AppointmentDate, a.SessionLink, a.Status, a.Fee, a.PaidStatus, a.Mobile, a.Age, a.Gender, a.Notes, 
                       COALESCE(u.FullName, 'Unknown Doctor') AS DoctorName 
                FROM Appointments a
                LEFT JOIN Users u ON a.DoctorId = u.Id
                WHERE a.PatientId = @PatientId 
                  AND a.AppointmentDate >= CAST(GETDATE() AS DATE)
                ORDER BY a.AppointmentDate ASC, a.SlotTime ASC";
            using var conn = _context.CreateConnection();
            var appointments = await conn.QueryAsync<Appointment>(query, new { PatientId = patientId });
            return Ok(appointments);
        }

    }
}
