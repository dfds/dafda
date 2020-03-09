using System;
using System.Threading.Tasks;
using InProcessOutbox.Domain;

namespace InProcessOutbox.Infrastructure.Persistence
{
    public class RelationalStudentRepository : IStudentRepository
    {
        private readonly SampleDbContext _dbContext;

        public RelationalStudentRepository(SampleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ValueTask<Student> FindById(Guid studentId)
        {
            return _dbContext.Students.FindAsync(studentId);
        }

        public async Task Save(Student student)
        {
            await _dbContext.Students.AddAsync(student);
        }
    }
}