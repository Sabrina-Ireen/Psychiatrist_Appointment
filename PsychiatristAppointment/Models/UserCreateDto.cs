// Models/UserCreateDto.cs
namespace PsychiatristAppointment.Models
{
    public class UserCreateDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Specialization { get; set; } = ""; // for doctors
        public string Role { get; set; } = "Doctor";
        public string Password { get; set; } = "";        // plain password from Swagger
    }
}
