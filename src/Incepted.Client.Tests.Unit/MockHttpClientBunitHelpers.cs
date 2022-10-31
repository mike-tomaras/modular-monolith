using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Incepted.Client.Tests.Unit;

internal static class MockHttpClientBunitHelpers
{
    public static MockHttpMessageHandler AddMockHttpClient(this TestServiceProvider services)
    {
        var mockHttpHandler = new MockHttpMessageHandler();
        var httpClient = mockHttpHandler.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost");
        services.AddSingleton(httpClient);
        return mockHttpHandler;
    }

    public static MockedRequest RespondJson<T>(this MockedRequest request, T content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        request.Respond(req =>
        {
            var jsonOptions = new JsonSerializerOptions();
            //to add json converters etc
            var response = new HttpResponseMessage(statusCode);
            response.Content = new StringContent(JsonSerializer.Serialize(content, jsonOptions));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return response;
        });
        return request;
    }

    public static MockedRequest RespondString(this MockedRequest request, string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        request.Respond(req =>
        {
            var response = new HttpResponseMessage(statusCode);
            response.Content = new StringContent(content);            
            return response;
        });
        return request;
    }
}
