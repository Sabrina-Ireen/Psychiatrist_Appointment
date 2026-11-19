namespace PsychiatristAppointment.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = "";
        public int PatientId { get; set; }
        public string PatientName { get; set; } = "";
        public string SlotTime { get; set; } = "";   // string
        public DateTime AppointmentDate { get; set; }
        public string SessionLink { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public decimal Fee { get; set; }  // new
        public string PaidStatus { get; set; } = "Pending"; // new
        public string Mobile { get; set; } = ""; // new
        public string Age { get; set; } = ""; // new
        public string Gender { get; set; } = ""; // new
        public string Notes { get; set; } = ""; // new

    }
}
