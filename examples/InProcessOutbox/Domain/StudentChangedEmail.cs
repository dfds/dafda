namespace InProcessOutbox.Domain
{
    public class StudentChangedEmail
    {
        public string StudentId { get; set; }
        public string OldEmail { get; set; }
        public string NewEmail { get; set; }
    }
}