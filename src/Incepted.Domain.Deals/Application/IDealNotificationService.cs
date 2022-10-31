using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Optional;
using System.Collections.Immutable;

namespace Incepted.Domain.Deals.Application;

public interface IDealNotificationService
{
    Option<Unit, ErrorCode> SendNotification(NotificationType type, IImmutableList<RecipientDTO> receipients, IImmutableDictionary<string, string> data);
}
