using Incepted.Db.DataModels.SharedDMs;
using Incepted.Domain.Companies.Entities;
using Incepted.Shared.ValueTypes;
using System.Net.Mail;

namespace Incepted.Db.DataModels.CompanyDMs;

internal class EmployeeDM : BaseDM
{
    public string UserId { get; set; }
    public HumanNameDM Name { get; set; }
    public string Email { get; set; }

    public static class Factory
    {
        public static EmployeeDM ToDataModel(Employee employee) => 
            new EmployeeDM
            {
                Version = 1,
                Id = employee.Id,
                UserId = employee.UserId.ToString(),
                Name = HumanNameDM.Factory.ToDataModel(employee.Name),
                Email = employee.Email.ToString()
            };

        public static Employee ToEntity(EmployeeDM employee) =>
            new Employee(
                employee.Id,
                new UserId(employee.UserId),
                HumanNameDM.Factory.ToEntity(employee.Name),
                new MailAddress(employee.Email)
            );
    }
}
