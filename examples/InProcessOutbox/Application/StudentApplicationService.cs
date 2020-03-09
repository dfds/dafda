using System;
using System.Threading.Tasks;
using InProcessOutbox.Domain;
using Microsoft.Extensions.Logging;

namespace InProcessOutbox.Application
{
    public class StudentApplicationService
    {
        private readonly ILogger<StudentApplicationService> _logger;
        private readonly IStudentRepository _studentRepository;
        private readonly IDomainEvents _domainEvents;

        public StudentApplicationService(ILogger<StudentApplicationService> logger, IStudentRepository studentRepository, IDomainEvents domainEvents)
        {
            _logger = logger;
            _studentRepository = studentRepository;
            _domainEvents = domainEvents;
        }

        public async Task<Student> StudentEntity(Guid studentId)
        {
            _logger.LogInformation("Fetching Student details for StudentId: {StudentId}", studentId);

            var student = await _studentRepository.FindById(studentId);
            if (student == null)
            {
                throw new Exception("Student not found");
            }

            return student;
        }

        public async Task<Student> EnrollStudent(string name, string email, string address)
        {
            _logger.LogInformation("Enroll Student {}", name);

            var studentEntity = new Student(name, email, address);

            await _studentRepository.Save(studentEntity);

            // publish the event
            _domainEvents.Raise(new StudentEnrolled
            {
                StudentId = studentEntity.Id.ToString(),
                Name = studentEntity.Name,
                Email = studentEntity.Email
            });

            return studentEntity;
        }

        public async Task<Student> ChangeStudentEmail(Guid studentId, string newStudentEmail)
        {
            _logger.LogInformation("Update Email to {NewEmail} for StudentId: {StudentId}", newStudentEmail, studentId);

            var student = await _studentRepository.FindById(studentId);
            var domainEvent = student.ChangeEmail(newStudentEmail);

            // publish the event
            _domainEvents.Raise(domainEvent);
            return student;
        }
    }
}