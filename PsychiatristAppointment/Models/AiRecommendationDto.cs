namespace PsychiatristAppointment.Models
{
    public class DoctorRecommendationDto
    {
        public int Id { get; set; }             // doctor id
        public string Name { get; set; }        // doctor full name
        public string Specialization { get; set; }
        public int Appointments { get; set; }   // number of appointments
    }


}
