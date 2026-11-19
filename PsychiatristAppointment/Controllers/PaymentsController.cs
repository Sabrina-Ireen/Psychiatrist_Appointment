using Dapper;
using Microsoft.AspNetCore.Mvc;
using PsychiatristAppointment.Context;
using PsychiatristAppointment.Models;

namespace PsychiatristAppointment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly DapperContext _context;
        public PaymentsController(DapperContext context) => _context = context; 

        // Create a payment record
        [HttpPost("Create")]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        {
            using var conn = _context.CreateConnection();

            var insertPayment = @"
        INSERT INTO Payments 
        (AppointmentId, PatientId, Amount, Status, PaymentDate)
        VALUES 
        (@AppointmentId, @PatientId, @Amount, 'Paid', GETDATE());
    ";

            var updateAppointment = @"
        UPDATE Appointments
        SET PaidStatus = 'Paid'
        WHERE AppointmentId = @AppointmentId;
    ";
            
            await conn.ExecuteAsync(insertPayment, payment);
            await conn.ExecuteAsync(updateAppointment, payment);

            return Ok(new { message = "Payment successful" });
        }


        // Update payment status (after success)
       
        // Get payments for a patient
        [HttpGet("Patient/{patientId}")]
        public async Task<IActionResult> GetPaymentsForPatient(int patientId)
        {
            var query = "SELECT * FROM Payments WHERE PatientId=@patientId ORDER BY PaymentDate DESC";
            using var conn = _context.CreateConnection();
            var payments = await conn.QueryAsync<Payment>(query, new { patientId });
            return Ok(payments);
        }
    }
}
