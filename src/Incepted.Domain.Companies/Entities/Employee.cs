using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using Optional;
using System.Net.Mail;

namespace Incepted.Domain.Companies.Entities;

public class Employee
{
    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public HumanName Name { get; private set; }
    public MailAddress Email { get; private set; }

    public Employee(Guid id, UserId userId, HumanName name, MailAddress email)
    {
        if (id == Guid.Empty) throw new ArgumentException("Employee Id can't be empty", $"{nameof(Employee)} {nameof(id)}");

        Id = id;
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public Option<Employee, ErrorCode> Update(UserDTO updateDTO)
    {
        HumanName newName = null;

        return Name.Update(updateDTO.FirstName, updateDTO.LastName)
            .Map(name => newName = name)
            .FlatMap(_ => ParseEmail(updateDTO.Email))
            .Map(newEmail => new Employee(Id, UserId, newName, newEmail));
    }

    private Option<MailAddress, ErrorCode> ParseEmail(string email)
    {
        var success = MailAddress.TryCreate(email, out var newEmail);

        return success ? newEmail.Some<MailAddress, ErrorCode>() : Option.None<MailAddress, ErrorCode>(CompanyErrorCodes.InvalidEmail);
    }

    public static class Factory
    {
        public static EmployeeDTO ToDTO(Employee employee)
        {
            return new EmployeeDTO(employee.Id, employee.UserId.ToString(), employee.Name.First, employee.Name.Last, employee.Email.Address);
        }
    }    
}

public class EmptyEmployee : Employee
{
    public EmptyEmployee() : base(Guid.NewGuid(), new UserId("auth0|abc"), new HumanName("empty", "empty"), new MailAddress("empty@empty.com"))
    { }
}
