namespace PsychiatristAppointment.Models
{
    public class DoctorSessionDto
    {
        public int AppointmentId { get; set; }      // from Appointments
        public string PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string SlotTime { get; set; }        // string in DB
        public string Diagnosis { get; set; }       // from Prescriptions
        public string Notes { get; set; }           // from Prescriptions
        public string Medicines { get; set; }       // new column
        public string SessionLink { get; set; }
    }


}
