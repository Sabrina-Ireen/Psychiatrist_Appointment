using System.Collections.Generic;

namespace PsychiatristAppointment.Models
{
    public class PatientDashboardDto
    {
        public User Patient { get; set; }
        public IEnumerable<Appointment> UpcomingAppointments { get; set; }
        public IEnumerable<PrescriptionResponseDto> RecentPrescriptions { get; set; }
        public DoctorProfileDto TopRecommendation { get; set; }
    }
}
