using Incepted.Domain.Deals.Application;
using Incepted.Notifications.MailerSend;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Incepted.Notifications;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    public static void With(this IServiceCollection services)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environmentName == "Development")
        {
            services.AddScoped<IDealNotificationService, DevelopmentNotificationService>();
            services.AddScoped<ICompanyNotificationService, DevelopmentNotificationService>();
        }
        else
        {
            var apiKey = Environment.GetEnvironmentVariable("MailerSend__ApiKey");
            services.AddScoped<IDealNotificationService, MailerSendNotificationService>(provider => new MailerSendNotificationService(apiKey));
            services.AddScoped<ICompanyNotificationService, MailerSendNotificationService>(provider => new MailerSendNotificationService(apiKey));
        }
    }
}
