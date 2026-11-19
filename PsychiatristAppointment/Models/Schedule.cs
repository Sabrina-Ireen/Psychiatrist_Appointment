
namespace PsychiatristAppointment.Models
{
    public class Schedule
    {
        public int DoctorId { get; set; }  // keep int
        public string Day { get; set; } = "";  // string
        public string StartTime { get; set; } = "";  // string
        public string EndTime { get; set; } = "";    // string
    }
}
