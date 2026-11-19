namespace PsychiatristAppointment.Models
{
    public class PrescriptionDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string Diagnosis { get; set; }
        public string Notes { get; set; }
        public string Medicines { get; set; }

    }
}
