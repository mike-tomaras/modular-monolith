using Incepted.Shared;
using MudBlazor;
using Optional;
using Serilog;
using System.Net.Http.Json;
using System.Text.Json;

namespace Incepted.Client.Extensions;

internal static class HttpExtensions
{
    public static async Task NotifyUserOfErrorsAsync(this HttpResponseMessage response, ISnackbar snackBar)
    {
        var errorJson = await response.Content.ReadAsStringAsync();
        try
        {
            if (string.IsNullOrEmpty(errorJson)) 
                snackBar.Add("Something went wrong! Please contact Incepted support if the problem persists.", Severity.Error);

            var errorDto = JsonSerializer.Deserialize<ErrorCode>(errorJson);
            errorDto?.errors.name.ForEach(message => snackBar.Add(message, Severity.Error));
        }
        catch (JsonException jsonex)
        {
            Log.Error(jsonex, "Error while deserializing error json on the client. Json: {Data}", errorJson);
            snackBar.Add("Something went wrong! Please contact Incepted support if the problem persists.", Severity.Error);
        }        
    }

    public static async Task<Option<T>> GetFromApiAsync<T>(this HttpClient client, string path, ISnackbar snackbar)
    {
        var response = await client.GetAsync(path);

        if (response.IsSuccessStatusCode)
        {
            var jsonOptions = new JsonSerializerOptions();
            //to add json converters etc
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(responseJson, jsonOptions);
            return result.SomeNotNull();
        }
        else
        {
            await response.NotifyUserOfErrorsAsync(snackbar);
            return Option.None<T>();
        }
    }

    public static async Task<Option<Guid>> PostToApiAsync(this HttpClient client, string path, HttpContent content, ISnackbar snackbar)
    {
        var response = await client.PostAsync(path, content);

        if (response.IsSuccessStatusCode)
        {
            var stringId = await response.Content.ReadAsStringAsync();
            var newResourceId = Guid.Parse(stringId.Trim('"'));
            return newResourceId.Some();
        }
        else
        {
            await response.NotifyUserOfErrorsAsync(snackbar);
            return Option.None<Guid>();
        }
    }

    public static async Task<Option<HttpResponseMessage>> PostToApiAsync<T>(this HttpClient client, string path, T content, ISnackbar snackbar)
    {
        var response = await client.PostAsJsonAsync(path, content);

        if (response.IsSuccessStatusCode)
        {
            return response.Some();
        }
        else
        {
            await response.NotifyUserOfErrorsAsync(snackbar);
            return Option.None<HttpResponseMessage>();
        }
    }

    public static async Task<Option<Unit>> PutToApiAsync<T>(this HttpClient client, string path, T content, ISnackbar snackbar)
    {
        var jsonOptions = new JsonSerializerOptions();
        //to add json converters etc
        var response = await client.PutAsJsonAsync(path, content, jsonOptions);

        if (response.IsSuccessStatusCode)
        {
            return new Unit().Some();
        }
        else
        {
            await response.NotifyUserOfErrorsAsync(snackbar);
            return Option.None<Unit>();
        }        
    }

    public static async Task<Option<Unit>> DeleteFromApiAsync(this HttpClient client, string path, ISnackbar snackbar)
    {
        var response = await client.DeleteAsync(path);

        if (response.IsSuccessStatusCode)
        {
            return new Unit().Some();
        }
        else
        {
            await response.NotifyUserOfErrorsAsync(snackbar);
            return Option.None<Unit>();
        }
    }
}
