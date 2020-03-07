using System;
using System.Threading.Tasks;

namespace InProcessOutbox.Domain
{
    public interface IStudentRepository
    {
        ValueTask<StudentEntity> FindById(Guid studentId);
        Task Save(StudentEntity studentEntity);
    }
}