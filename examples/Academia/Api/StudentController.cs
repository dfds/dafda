using System;
using System.Threading.Tasks;
using Academia.Application;
using Academia.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Academia.Controllers
{
    [ApiController]
    [Route("api")]
    public class StudentController : ControllerBase
    {
        private readonly StudentApplicationService _studentService;

        public StudentController(StudentApplicationService studentService)
        {
            _studentService = studentService;
        }

        [HttpGet("students/{studentId:Guid}")]
        public async Task<ActionResult<StudentDto>> GetStudent([FromRoute] Guid studentId)
        {
            var student = await _studentService.StudentEntity(studentId);

            var studentDto = MapStudentToDto(student);

            return Ok(studentDto);
        }

        [Transactional]
        [HttpPost("students/~/enroll")]
        public async Task<ActionResult<StudentDto>> EnrollStudent([FromBody] EnrollStudentInput input)
        {
            var student = await _studentService.EnrollStudent(input.Name, input.Email, input.Address);

            var studentDto = MapStudentToDto(student);

            return Ok(studentDto);
        }

        [Transactional]
        [HttpPost("students/{studentId:Guid}/update-email")]
        public async Task<ActionResult<StudentDto>> UpdateStudentEmail([FromRoute] Guid studentId, [FromBody] ChangeEmailInput input)
        {
            var student = await _studentService.ChangeStudentEmail(studentId, input.Email);

            var studentDto = MapStudentToDto(student);

            return Ok(studentDto);
        }

        private static StudentDto MapStudentToDto(Student student)
        {
            return new StudentDto
            {
                StudentId = student.Id,
                Name = student.Name,
                Email = student.Email,
                Address = student.Address
            };
        }
    }
}