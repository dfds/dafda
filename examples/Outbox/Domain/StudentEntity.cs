using System;

namespace Outbox.Domain
{
    public class StudentEntity
    {
        public StudentEntity(string name, string email, string address)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Address = address;
        }

        public Guid Id { get; }
        public string Name { get; }
        public string Email { get; private set; }
        public string Address { get; }

        public StudentChangedEmail ChangeEmail(string newEmail)
        {
            if (string.Equals(Email, newEmail, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var oldEmail = Email;
            Email = newEmail;

            return new StudentChangedEmail()
            {
                StudentId = Id.ToString(),
                OldEmail = oldEmail,
                NewEmail = newEmail
            };
        }
    }
}