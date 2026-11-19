namespace PsychiatristAppointment.Models
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }          // maps from Users.FullName
        public string Specialization { get; set; }
        public string AvailableDays { get; set; }  // aggregated from Schedules table
        public string Email { get; set; }
        public decimal Fee { get; set; }
    }
}
