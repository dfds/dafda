using System;

namespace InProcessOutbox.Controllers
{
    public class StudentDto
    {
        public Guid StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}