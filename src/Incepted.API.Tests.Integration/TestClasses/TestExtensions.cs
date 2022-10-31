using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Incepted.API.Tests.Integration.TestClasses;

internal static class TestExtensions
{
    public static async Task<T> GetContentAsync<T>(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
    }

    public static T Get<T>(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var result = scope.ServiceProvider.GetService<T>();

        if (result == null) throw new ArgumentException($"Can't resolve service for integration test");

        return result;
    }
}
