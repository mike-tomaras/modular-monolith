using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using System.Net.Mail;

namespace Incepted.Domain.Deals.Domain;

public class Assignee
{
    public Guid Id { get; private set; }
    public UserId UserId { get; private set; }
    public HumanName Name { get; private set; }
    public MailAddress Email { get; private set; }

    public Assignee(Guid id, UserId userId, HumanName name, MailAddress email)
    {
        if (id == Guid.Empty) throw new ArgumentException("Company Id can't be empty", $"{nameof(Assignee)} {nameof(id)}");

        Id = id;
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    internal static class Factory
    {
        public static Assignee Empty => new Assignee(Guid.NewGuid(), new UserId("auth0|empty"), new HumanName("Empty", "Empty"), new MailAddress("empty@empty.com"));

        public static Assignee ToEntity(EmployeeDTO employeeDTO)
        {
            return new Assignee(employeeDTO.Id, new UserId(employeeDTO.UserId), new HumanName(employeeDTO.FirstName, employeeDTO.LastName), new MailAddress(employeeDTO.Email));
        }

        public static EmployeeDTO ToEntity(Assignee assignee)
        {
            return new EmployeeDTO(assignee.Id, assignee.UserId.ToString(), assignee.Name.First, assignee.Name.Last, assignee.Email.ToString());
        }
    }
}
