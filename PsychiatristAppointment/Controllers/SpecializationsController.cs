using Dapper;
using Microsoft.AspNetCore.Mvc;
using PsychiatristAppointment.Context;
using PsychiatristAppointment.Models;

namespace PsychiatristAppointment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecializationsController : ControllerBase
    {
        private readonly DapperContext _context;

        public SpecializationsController(DapperContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            using var conn = _context.CreateConnection();
            var specializations = await conn.QueryAsync<SpecializationDto>("SELECT * FROM Specializations");
            return Ok(specializations);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] SpecializationDto dto)
        {
            using var conn = _context.CreateConnection();
            var query = "INSERT INTO Specializations (Name) VALUES (@Name)";
            await conn.ExecuteAsync(query, dto);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SpecializationDto dto)
        {
            using var conn = _context.CreateConnection();
            var query = "UPDATE Specializations SET Name=@Name WHERE Id=@Id";
            await conn.ExecuteAsync(query, new { dto.Name, Id = id });
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var conn = _context.CreateConnection();
            var query = "DELETE FROM Specializations WHERE Id=@Id";
            await conn.ExecuteAsync(query, new { Id = id });
            return Ok();
        }
    }

}
