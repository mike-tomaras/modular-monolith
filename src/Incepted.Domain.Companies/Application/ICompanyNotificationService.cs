using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using Optional;

namespace Incepted.Domain.Deals.Application;

public interface ICompanyNotificationService
{
    Option<Unit, ErrorCode> NotifyAdminOfUserDetailsChangeRequest(UserId userId, UserDTO userDTO);
}
