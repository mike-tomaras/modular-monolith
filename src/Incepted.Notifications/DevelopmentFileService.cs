using Incepted.Domain.Deals.Application;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using Optional;
using Serilog;
using System.Collections.Immutable;

namespace Incepted.Notifications
{
    internal class DevelopmentNotificationService : IDealNotificationService, ICompanyNotificationService
    {
        public Option<Unit, ErrorCode> NotifyAdminOfUserDetailsChangeRequest(UserId userId, UserDTO userDTO)
        {
            Log.Information(
                    "Sending notification to admins to update user details for user ID {UserId} and details: first name {FirstName}, last name {LastName}, email {Email}.",
                    userId,
                    userDTO.FirstName,
                    userDTO.LastName,
                    userDTO.Email
                    );

            return new Unit().Some<Unit, ErrorCode>();
        }

        public Option<Unit, ErrorCode> SendNotification(NotificationType type, IImmutableList<RecipientDTO> receipients, IImmutableDictionary<string, string> data)
        {
            var dataString = data.Aggregate(string.Empty, (current, next) => $"{current},{next.Key}={next.Value}").Trim(',');
            foreach (var recipient in receipients)
            {
                Log.Information(
                    "Sending notification of type {NotificationType}, to {NotificationRecipient}, with data {NotificationData}.",
                    type,
                    recipient,
                    dataString
                    );
            }

            return new Unit().Some<Unit, ErrorCode>();
        }
    }
}