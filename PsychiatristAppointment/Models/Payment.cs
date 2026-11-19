namespace PsychiatristAppointment.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}
