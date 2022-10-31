using Incepted.Db.DataModels.SharedDMs;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using System.Net.Mail;

namespace Incepted.Db.DataModels.DealDMs;

internal class AssigneeDM : BaseDM
{
    public string UserId { get; set; }
    public HumanNameDM Name { get; set; }
    public string Email { get; set; }

    public static class Factory
    {
        public static AssigneeDM ToDataModel(Assignee assignee) =>
            new AssigneeDM
            {
                Version = 1,
                Id = assignee.Id,
                UserId = assignee.UserId.ToString(),
                Name = HumanNameDM.Factory.ToDataModel(assignee.Name),                
                Email = assignee.Email.ToString()
            };

        public static Assignee ToEntity(AssigneeDM assignee) =>
            new Assignee(
                assignee.Id,
                new UserId(assignee.UserId),
                HumanNameDM.Factory.ToEntity(assignee.Name),
                new MailAddress(assignee.Email)
            );
        public static EmployeeDTO ToDTO(AssigneeDM assignee) =>
            new EmployeeDTO(
                assignee.Id,
                assignee.UserId.ToString(),
                assignee.Name.First,
                assignee.Name.Last,
                assignee.Email
            );
    }
}
