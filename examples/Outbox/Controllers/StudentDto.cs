using System;

namespace Outbox.Controllers
{
    public class StudentDto
    {
        public Guid StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}