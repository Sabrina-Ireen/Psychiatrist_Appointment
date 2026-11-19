using System;

namespace PsychiatristAppointment.Models
{
    public class PrescriptionResponseDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = "";
        public string Diagnosis { get; set; } = "";
        public string Notes { get; set; } = "";
        public string Medicines { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
