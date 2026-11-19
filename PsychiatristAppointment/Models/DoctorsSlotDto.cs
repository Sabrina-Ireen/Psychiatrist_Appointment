    namespace PsychiatristAppointment.Models
    {
        public class DoctorSlotDto
        {
            public int PatientId { get; set; }
            public string PatientName { get; set; } = "";
            public DateTime AppointmentDate { get; set; }
            public string SlotTime { get; set; } = "";  // string
            public string SessionLink { get; set; } = "";
        }
    }
