using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using PsychiatristAppointment.Context;
using PsychiatristAppointment.Models;
using System.Security.Cryptography;
using System.Text;

namespace PsychiatristAppointment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DapperContext _context;

        public AdminController(DapperContext context)
        {
            _context = context;
        }
        private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(password)).SequenceEqual(hash);
        }

        // ------------------------------
        // GET all patients
        // ------------------------------
        [HttpGet("Patients")]
        public async Task<IActionResult> GetAllPatients()
        {
            var query = @"SELECT Id, FullName, Email, Role, CreatedAt 
                          FROM Users 
                          WHERE Role='Patient'";

            using var connection = _context.CreateConnection();
            var patients = await connection.QueryAsync(query);
            return Ok(patients);
        }
        // ------------------------------
        // ADD a patient
        // ------------------------------
        [HttpPost("Patients")]
        public async Task<IActionResult> AddPatient([FromBody] UserCreateDto dto)
        {
            CreatePasswordHash(dto.Password, out var passwordHash, out var passwordSalt);

            var patient = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Role = "Patient",
                CreatedAt = DateTime.Now,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var query = @"INSERT INTO Users (FullName, Email, Role, CreatedAt, PasswordHash, PasswordSalt)
                  VALUES (@FullName, @Email, @Role, @CreatedAt, @PasswordHash, @PasswordSalt);
                  SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = _context.CreateConnection();
            patient.Id = await connection.QuerySingleAsync<int>(query, patient);

            return Ok(patient);
        }

    
        // ------------------------------
        // UPDATE a patient
        // ------------------------------
        [HttpPut("Patients/{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] UserUpdateDto dto)
        {
            var query = @"UPDATE Users 
                  SET FullName = @FullName,
                      Email = @Email
                  WHERE Id = @Id AND Role='Patient'";

            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteAsync(query, new { dto.FullName, dto.Email, Id = id });

            if (result > 0) return Ok(dto);
            return NotFound("Patient not found");
        }


        // ------------------------------
        // DELETE a patient
        // ------------------------------
        [HttpDelete("Patients/{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var query = @"DELETE FROM Users WHERE Id=@Id AND Role='Patient'";
            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteAsync(query, new { Id = id });
            if (result > 0) return Ok(new { msg = "Deleted successfully" });
            return NotFound("Patient not found");
        }


        // ------------------------------
        // GET all doctors
        // ------------------------------
        [HttpGet("Doctors")]
        public async Task<IActionResult> GetAllDoctors()
        {
            try
            {
                var query = @"SELECT Id, FullName, Email, Specialization, Role, IsActive, Fee, CreatedAt 
                              FROM Users 
                              WHERE TRIM(Role)='Doctor'";

                using var connection = _context.CreateConnection();
                var doctors = await connection.QueryAsync<DoctorProfileDto>(query);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // ------------------------------
        // ADD a doctor
        // ------------------------------
        [HttpPost("Doctors")]
        public async Task<IActionResult> AddDoctor([FromBody] UserCreateDto dto)
        {
            // 1️⃣ Hash the password using your helper
            CreatePasswordHash(dto.Password, out var passwordHash, out var passwordSalt);

            // 2️⃣ Prepare User object
            var doctor = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Specialization = dto.Specialization,
                Role = "Doctor",
                CreatedAt = DateTime.Now,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            // 3️⃣ Insert into DB
            var query = @"INSERT INTO Users (FullName, Email, Specialization, Role, CreatedAt, PasswordHash, PasswordSalt)
                  VALUES (@FullName, @Email, @Specialization, @Role, @CreatedAt, @PasswordHash, @PasswordSalt);
                  SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = _context.CreateConnection();
            doctor.Id = await connection.QuerySingleAsync<int>(query, doctor);

            return Ok(doctor);
        }



        // ------------------------------
        // UPDATE a doctor
        // ------------------------------
        [HttpPut("Doctors/{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] UserUpdateDto dto)
        {
            var query = @"UPDATE Users 
                  SET FullName = @FullName,
                      Email = @Email,
                      Specialization = @Specialization
                  WHERE Id = @Id AND Role='Doctor'";

            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteAsync(query, new { dto.FullName, dto.Email, dto.Specialization, Id = id });

            if (result > 0) return Ok(dto);
            return NotFound("Doctor not found");
        }


        // ------------------------------
        // DELETE a doctor
        // ------------------------------
        [HttpDelete("Doctors/{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var query = @"DELETE FROM Users WHERE Id=@Id AND Role='Doctor'";
            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteAsync(query, new { Id = id });
            if (result > 0) return Ok(new { msg = "Deleted successfully" });
            return NotFound("Doctor not found");
        }

        // ------------------------------
        // GET all appointments
        // ------------------------------
        [HttpGet("Appointments")]
        public async Task<IActionResult> GetAllAppointments()
        {
            var query = @"
SELECT a.AppointmentId, a.DoctorId, d.FullName AS DoctorName,
       a.PatientId, p.FullName AS PatientName,
       a.SlotTime, a.AppointmentDate, a.SessionLink, a.Status
FROM Appointments a
INNER JOIN Users d ON a.DoctorId = d.Id
INNER JOIN Users p ON a.PatientId = p.Id
ORDER BY a.AppointmentDate DESC, a.SlotTime DESC";

            using var connection = _context.CreateConnection();
            var appointments = await connection.QueryAsync(query);
            return Ok(appointments);
        }
        // Add appointment
        [HttpPost("Appointments")]
        public async Task<IActionResult> AddAppointment([FromBody] Appointment dto)
        {
            var query = @"INSERT INTO Appointments (DoctorId, PatientId, SlotTime, AppointmentDate, SessionLink, Status)
                  VALUES (@DoctorId, @PatientId, @SlotTime, @AppointmentDate, @SessionLink, @Status);
                  SELECT CAST(SCOPE_IDENTITY() as int)";

            using var connection = _context.CreateConnection();
            var id = await connection.QuerySingleAsync<int>(query, dto);

            return Ok(new { AppointmentId = id, dto.DoctorId, dto.PatientId, dto.SlotTime, dto.AppointmentDate, dto.SessionLink, dto.Status });
        }


        // Update appointment
        [HttpPut("Appointments/{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] Appointment dto)
        {
            var query = @"UPDATE Appointments
                  SET DoctorId = @DoctorId,
                      PatientId = @PatientId,
                      SlotTime = @SlotTime,
                      AppointmentDate = @AppointmentDate,
                      SessionLink = @SessionLink,
                      Status = @Status
                  WHERE AppointmentId = @Id";

            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteAsync(query, new { dto.DoctorId, dto.PatientId, dto.SlotTime, dto.AppointmentDate, dto.SessionLink, dto.Status, Id = id });
            if (result > 0) return Ok(dto);
            return NotFound("Appointment not found");
        }

        // Delete appointment
        [HttpDelete("Appointments/{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var query = @"DELETE FROM Appointments WHERE AppointmentId=@Id";
            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteAsync(query, new { Id = id });
            if (result > 0) return Ok("Deleted successfully");
            return NotFound("Appointment not found");
        }


        // ------------------------------
        // GET counts
        // ------------------------------
        [HttpGet("Counts")]
        public async Task<IActionResult> GetCounts()
        {
            using var connection = _context.CreateConnection();

            var doctorCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE Role='Doctor'");
            var patientCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users WHERE Role='Patient'");
            var appointmentCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Appointments");
            var scheduleCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Schedules");

            return Ok(new
            {
                totalDoctors = doctorCount,
                totalPatients = patientCount,
                totalAppointments = appointmentCount,
                totalSchedules = scheduleCount
            });
        }

        // ------------------------------
        // GET all schedules
        // ------------------------------
        [HttpGet("Schedules")]
        public async Task<IActionResult> GetSchedules()
        {
            var sql = @"
        SELECT 
            s.ScheduleId,
            s.DoctorId,
            u.FullName AS DoctorName,
            s.Day,
            s.StartTime,
            s.EndTime
        FROM Schedules s
        INNER JOIN Users u ON s.DoctorId = u.Id
        WHERE u.Role = 'Doctor'";

            using var connection = _context.CreateConnection();
            var data = await connection.QueryAsync(sql);
            return Ok(data);
        }
        [HttpPost("Schedules")]
        public async Task<IActionResult> AddSchedule([FromBody] Schedule schedule)
        {
            var sql = @"
        INSERT INTO Schedules (DoctorId, Day, StartTime, EndTime)
        VALUES (@DoctorId, @Day, @StartTime, @EndTime)";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, schedule);

            return Ok(schedule);
        }
        [HttpPut("Schedules/{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Schedule schedule)
        {
            var sql = @"
        UPDATE Schedules
        SET DoctorId=@DoctorId,
            Day=@Day,
            StartTime=@StartTime,
            EndTime=@EndTime
        WHERE ScheduleId=@Id";

            using var connection = _context.CreateConnection();
            var rows = await connection.ExecuteAsync(sql, new
            {
                schedule.DoctorId,
                schedule.Day,
                schedule.StartTime,
                schedule.EndTime,
                Id = id
            });

            if (rows == 0) return NotFound();
            return Ok(schedule);
        }
        [HttpDelete("Schedules/{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var sql = "DELETE FROM Schedules WHERE ScheduleId=@Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });

            return Ok();
        }



        // ------------------------------
        // GET Reports
        // ------------------------------
        [HttpGet("Reports")]
        public async Task<IActionResult> GetReports()
        {
            using var connection = _context.CreateConnection();

            var totalAppointments = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Appointments");
            var completedAppointments = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Appointments WHERE Status='Completed'");
            var pendingAppointments = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Appointments WHERE Status='Pending'");
            var cancelledAppointments = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Appointments WHERE Status='Cancelled'");

            var topDoctors = await connection.QueryAsync(@"
    SELECT 
        u.FullName AS DoctorName,
        u.Specialization,
        COUNT(a.AppointmentId) AS TotalAppointments
    FROM Appointments a
    JOIN Users u ON a.DoctorId = u.Id
    WHERE u.Role = 'Doctor'
    GROUP BY u.FullName, u.Specialization
    ORDER BY TotalAppointments DESC
");


            return Ok(new
            {
                totalAppointments = totalAppointments,
                completed = completedAppointments,
                pending = pendingAppointments,
                cancelled = cancelledAppointments,
                topDoctors = topDoctors
            });

        }
 
    }
}
