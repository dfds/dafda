using System;
using System.Threading.Tasks;

namespace Outbox.Domain
{
    public interface IStudentRepository
    {
        ValueTask<StudentEntity> FindById(Guid studentId);
        Task Save(StudentEntity studentEntity);
    }
}