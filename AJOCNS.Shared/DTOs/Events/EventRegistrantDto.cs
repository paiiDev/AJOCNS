namespace AJOCNS.Shared.DTOs.Events
{
    public class EventRegistrantDto
    {
        public int StudentId { get; set; }

        public string Name { get; set; } = null!;

        public string Srn { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime RegistrationDate { get; set; }
    }
}
