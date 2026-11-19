namespace PsychiatristAppointment.Models
{
    public class DoctorProfileDto
    {
        public int? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Specialization { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public decimal? Fee { get; set; } 
        public string? AvailableDays { get; set; }
        public DateTime? CreatedAt { get; set; }
    }


}
