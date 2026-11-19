using Dapper;
using Microsoft.AspNetCore.Mvc;
using PsychiatristAppointment.Context;
using PsychiatristAppointment.Models;

namespace PsychiatristAppointment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiQuizController : ControllerBase
    {
        private readonly DapperContext _context;

        public AiQuizController(DapperContext context)
        {
            _context = context;
        }

        [HttpGet("recommendation")]
        public async Task<IActionResult> GetRecommendation(string specialization)
        {
            using var connection = _context.CreateConnection();

            // ✅ Get doctors, their appointment counts, and their schedules dynamically
            var sql = @"
                SELECT TOP 1
                    u.Id,
                    u.FullName AS Name,
                    u.Specialization,
                    u.Email,
                    u.Fee,
                    (SELECT STRING_AGG(val, ', ') 
                     FROM (SELECT DISTINCT s.Day + ' (' + CONVERT(varchar(5), s.StartTime, 108) + '-' + CONVERT(varchar(5), s.EndTime, 108) + ')' AS val 
                           FROM Schedules s 
                           WHERE s.DoctorId = u.Id) AS t) AS AvailableDays
                FROM Users u
                WHERE u.Role = 'Doctor' AND u.IsActive = 1 AND u.Specialization = @Specialization
                ORDER BY (SELECT COUNT(*) FROM Appointments a WHERE a.DoctorId = u.Id) DESC
            ";
 
            var topDoctor = await connection.QueryFirstOrDefaultAsync<DoctorDto>(sql, new { Specialization = specialization });
 
            if (topDoctor == null)
                return NotFound("No doctors found for this specialization");
 
            return Ok(new { doctors = new List<DoctorDto> { topDoctor } });

        }
    }
}
