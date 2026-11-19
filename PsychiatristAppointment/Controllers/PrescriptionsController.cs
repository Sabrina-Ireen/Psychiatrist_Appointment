using Dapper;
using Microsoft.AspNetCore.Mvc;
using PsychiatristAppointment.Context;
using PsychiatristAppointment.Models;

namespace PsychiatristAppointment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly DapperContext _context;

        public PrescriptionsController(DapperContext context)
        {
            _context = context;
        }

        // ===== Add a new prescription =====
        [HttpPost]
        public async Task<IActionResult> AddPrescription([FromBody] PrescriptionDto prescription)
        {
            if (prescription == null)
                return BadRequest("Invalid data");

            using var connection = _context.CreateConnection();

            // 1️⃣ Find the appointment for this patient & doctor (most recent)
            var appointmentQuery = @"
        SELECT TOP 1 AppointmentId 
        FROM Appointments
        WHERE PatientId = @PatientId 
          AND DoctorId = @DoctorId
        ORDER BY AppointmentDate DESC, SlotTime DESC
    ";

            var appointmentId = await connection.QueryFirstOrDefaultAsync<int?>(
                appointmentQuery,
                new { prescription.PatientId, prescription.DoctorId }
            );

            if (appointmentId == null)
                return BadRequest("No appointment found for this patient with this doctor.");

            // 2️⃣ Insert the prescription
            var insertQuery = @"
        INSERT INTO Prescriptions (AppointmentId, PatientId, DoctorId, Diagnosis, Notes, Medicines)
        VALUES (@AppointmentId, @PatientId, @DoctorId, @Diagnosis, @Notes, @Medicines);
        SELECT CAST(SCOPE_IDENTITY() AS INT);
    ";

            var newId = await connection.QuerySingleAsync<int>(insertQuery, new
            {
                AppointmentId = appointmentId,
                prescription.PatientId,
                prescription.DoctorId,
                prescription.Diagnosis, // ✅ Added
                prescription.Notes,
                prescription.Medicines
            });

            // 3️⃣ Mark appointment as Completed
            var updateStatusQuery = "UPDATE Appointments SET Status = 'Completed' WHERE AppointmentId = @AppointmentId";
            await connection.ExecuteAsync(updateStatusQuery, new { AppointmentId = appointmentId });

            return Ok(new { Id = newId });
        }

        // ===== Get all prescriptions =====
        [HttpGet]
        public async Task<IActionResult> GetAllPrescriptions()
        {
            var query = @"
                SELECT Id, PatientId, DoctorId, Diagnosis, Notes, Medicines, CreatedAt
                FROM Prescriptions
                ORDER BY CreatedAt DESC
            ";

            using var connection = _context.CreateConnection();
            var prescriptions = await connection.QueryAsync(query);

            return Ok(prescriptions);
        }

        // ===== Get prescription by appointment =====
        [HttpGet("Appointment/{appointmentId}")]
        public async Task<IActionResult> GetPrescriptionByAppointment(int appointmentId)
        {
            var query = @"
                SELECT p.Id, p.PatientId, p.DoctorId, p.Diagnosis, p.Notes, p.Medicines, p.CreatedAt,
                       u.FullName AS DoctorName
                FROM Prescriptions p
                LEFT JOIN Users u ON p.DoctorId = u.Id
                WHERE p.AppointmentId = @AppointmentId
            ";

            using var connection = _context.CreateConnection();
            var prescription = await connection.QueryFirstOrDefaultAsync<PrescriptionResponseDto>(query, new { AppointmentId = appointmentId });

            if (prescription == null) return NotFound("Prescription not found");

            return Ok(prescription);
        }

        // ===== Get prescriptions by patient =====
        [HttpGet("Patient/{patientId}")]
        public async Task<IActionResult> GetPrescriptionsByPatient(int patientId)
        {
            var query = @"
                SELECT p.Id, p.PatientId, p.DoctorId, p.Diagnosis, p.Notes, p.Medicines, p.CreatedAt,
                       u.FullName AS DoctorName
                FROM Prescriptions p
                LEFT JOIN Users u ON p.DoctorId = u.Id
                WHERE p.PatientId = @PatientId
                ORDER BY p.CreatedAt DESC
            ";

            using var connection = _context.CreateConnection();
            var prescriptions = await connection.QueryAsync<PrescriptionResponseDto>(query, new { PatientId = patientId });

            return Ok(prescriptions);
        }

        // ===== Update prescription =====
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrescription(int id, [FromBody] PrescriptionDto prescription)
        {
            var query = @"
                UPDATE Prescriptions
                SET Diagnosis = @Diagnosis,
                    Notes = @Notes,
                    Medicines = @Medicines
                WHERE Id = @Id
            ";

            using var connection = _context.CreateConnection();
            var rows = await connection.ExecuteAsync(query, new
            {
                Id = id,
                prescription.Diagnosis,
                prescription.Notes,
                prescription.Medicines
            });

            if (rows == 0) return NotFound("Prescription not found");

            return Ok(new { message = "Prescription updated successfully" });
        }

        // ===== Delete prescription =====
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var query = "DELETE FROM Prescriptions WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            var rows = await connection.ExecuteAsync(query, new { Id = id });

            if (rows == 0) return NotFound("Prescription not found");

            return Ok(new { message = "Prescription deleted successfully" });
        }
    }
}
