using Incepted.Domain.Deals.Application;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using MoreLinq;
using Optional;
using Serilog;
using System.Collections.Immutable;
using System.Dynamic;
using System.Text;
using System.Text.Json;

namespace Incepted.Notifications.MailerSend;

internal class MailerSendNotificationService : IDealNotificationService, ICompanyNotificationService
{
    private readonly Dictionary<NotificationType, string> _emailTemplateIds =
        new()
        {
            { NotificationType.Insurer_NewSubmission, "3z0vklo6o11l7qrx" },
            { NotificationType.Insurer_Invite, "----" },
            { NotificationType.Insurer_SubmissionModified, "x2p0347zer3lzdrn" },
            { NotificationType.Broker_NewSubmissionFeedback, "jpzkmgq75vnl059v" },
            { NotificationType.Broker_SubmissionDeclined, "jpzkmgq75vnl059v" }, //TODO: create new template when we pay for more, this is the same as the new feedback
        };
    private HttpClient _httpClient;

    public MailerSendNotificationService(string apiKey)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api.mailersend.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public Option<Unit, ErrorCode> SendNotification(NotificationType type, IImmutableList<RecipientDTO> receipients, IImmutableDictionary<string, string> data)
    {
        //TODO: save all notifications to be sent in the db 
        
        var apiSendTasks = receipients
            .Batch(500)
            .Select(batch =>
            {
                var payload = GetPayload(type, batch, data);
                string json = JsonSerializer.Serialize(payload);

                return MakeApiCallAsync("bulk-email", json);//max 10 req/min allowed, might have to rate limit later
            });

        var results = Task.WhenAll(apiSendTasks).Result;
        results.ForEach(result => result.MatchSome(_ => { /*TODO: Mark the batch as accepted for sending*/ }));

        return results.Any(r => !r.HasValue) 
            ? Option.None<Unit, ErrorCode>(NotificationErrorCodes.FailedToSendNotification_Email) 
            : new Unit().Some<Unit, ErrorCode>();
    }
    private async Task<Option<Unit, ErrorCode>> MakeApiCallAsync(string path, string json)
    {
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var result = await _httpClient.PostAsync(path, content);
        
        if (!result.IsSuccessStatusCode)
        {
            var errorContent = await result.Content.ReadAsStringAsync();
            Log.Error("Error while sending notification. Reason: {ErrorStatus} | {ErrorDetails}", (int)result.StatusCode, errorContent);

            return new Unit().None(NotificationErrorCodes.FailedToSendNotification_Email);
        }

        return new Unit().Some<Unit, ErrorCode>();
    }
    private dynamic GetPayload(NotificationType type, IEnumerable<RecipientDTO> recipients, IImmutableDictionary<string, string> data)
    {
        dynamic payload = new ExpandoObject();
        payload = recipients.Select(recipient => GetSingleEmail(type, recipient, data));

        return payload;
    }
    private dynamic GetSingleEmail(NotificationType type, RecipientDTO recipient, IImmutableDictionary<string, string> data)
    {
        dynamic singleEmail = new ExpandoObject();
        singleEmail.to = new[] { GetAdressee(recipient) };
        singleEmail.personalization = new[] { GetAdresseePersonalization(recipient, data) };
        singleEmail.template_id = _emailTemplateIds[type];

        return singleEmail;
    }
    private dynamic GetAdressee(RecipientDTO recipient)
    {
        dynamic addressee = new ExpandoObject();
        addressee.email = recipient.Email;

        return addressee;
    }
    private dynamic GetAdresseePersonalization(RecipientDTO recipient, IImmutableDictionary<string, string> data)
    {
        var dataObj = new ExpandoObject() as IDictionary<string, Object>;
        dataObj.Add("name", recipient.FirstName);
        data.ForEach(pair => dataObj.Add(pair.Key, pair.Value));

        dynamic personalization = new ExpandoObject();
        personalization.email = recipient.Email;
        personalization.data = dataObj;

        return personalization;
    }

    public Option<Unit, ErrorCode> NotifyAdminOfUserDetailsChangeRequest(UserId userId, UserDTO userDTO)
    {        
        dynamic fromData = new ExpandoObject();
        fromData.email = "no-reply@incepted.io";
        fromData.name = "No reply";
        dynamic toData = new ExpandoObject();
        toData.email = "mike@incepted.io"; //TODO: make admin@incepted.io
        toData.name = "Admin Incepted";
        //dynamic varData = new ExpandoObject();
        //varData.email = "mike@incepted.io"; //TODO: make admin@incepted.io

        dynamic payload = new ExpandoObject();
        payload.from = fromData;
        payload.to = new[] { toData };
        payload.subject = "User data update request";
        payload.html = "<p>User requested to change their data.</p>" +
            "<p>Data:" +
            "   <ul>" +
            $"      <li>User id: {userId}</li>" +
            $"      <li>First name: {userDTO.FirstName}</li>" +
            $"      <li>Last name: {userDTO.LastName}</li>" +
            $"      <li>Email: {userDTO.Email}</li>" +
            "   </ul>" + 
            "</p>" +
            "<p>Please login to the <a href=\"https://manage.auth0.com/dashboard/eu/incepted-prod/users\">user portal</a> and make the necessary changes.</p>";
        //payload.variables = new[] { varData };

        string json = JsonSerializer.Serialize(payload);
        return MakeApiCallAsync("email", json).GetAwaiter().GetResult();
    }
}