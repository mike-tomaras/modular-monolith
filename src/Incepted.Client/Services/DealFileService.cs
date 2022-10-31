using Incepted.Client.Extensions;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using Optional;
using Serilog;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Incepted.Client.Services;

internal interface IDealFileService
{
    Task<Option<List<FileUploadResult>, ErrorCode>> UploadFilesAsync(InputFileChangeEventArgs e, Guid dealId, FileType type);
    Task<Option<FileUploadResult, ErrorCode>> UploadTCsAsync(InputFileChangeEventArgs e);
    Task DownloadFileAsync(Guid dealId, FileDTO file);
    Task DownloadFileAsync(byte[] bytes, string fileName);
    Task DownloadTCsAsync(FileDTO file);
}

internal class DealFileService : IDealFileService
{
    private readonly ISnackbar _snackbar;
    private readonly HttpClient _http;
    private IJSRuntime _js;

    public DealFileService(ISnackbar snackbar, HttpClient http, IJSRuntime js)
    {
        _snackbar = snackbar;
        _http = http;
        _js = js;
    }

    public async Task<Option<List<FileUploadResult>, ErrorCode>> UploadFilesAsync(InputFileChangeEventArgs e, Guid dealId, FileType type)
    {
        int maxAllowedFiles = 5;        
        var upload = false;
        IReadOnlyList<IBrowserFile>? files = null;
        List<FileUploadResult> uploadResults = new();

        try
        {
            files = e.GetMultipleFiles(maxAllowedFiles);
        }
        catch (Exception ex)
        {
            Log.Error("Could not read list of files. Exc: {Error}", ex.Message);
            _snackbar.Add($"You can only upload up to {maxAllowedFiles} files at one time.", Severity.Error);
            return Option.None<List<FileUploadResult>, ErrorCode>(FileErrorCodes.TooManyFiles);
        }

        using var content = new MultipartFormDataContent();

        files
            .ToList()
            .ForEach(file => AddToContent(file, type, content, uploadResults, ref upload));

        if (!upload) return Option.None<List<FileUploadResult>, ErrorCode>(FileErrorCodes.FileTooLarge);

        var response = await _http.PostAsync($"api/v1/deals/{dealId}/file/{type}", content);

        var newUploadResults = await response.Content.ReadFromJsonAsync<IList<FileUploadResult>>() ?? new List<FileUploadResult>();

        return uploadResults.Concat(newUploadResults).ToList().Some<List<FileUploadResult>, ErrorCode>();
    }

    public async Task<Option<FileUploadResult, ErrorCode>> UploadTCsAsync(InputFileChangeEventArgs e)
    {
        var upload = false;
        List<FileUploadResult> uploadResults = new();

        using var content = new MultipartFormDataContent();

        AddToContent(e.File, FileType.InsurerTCs, content, uploadResults, ref upload);

        if (!upload) return Option.None<FileUploadResult, ErrorCode>(FileErrorCodes.FileTooLarge);

        var response = await _http.PostAsync($"api/v1/company/tcs", content);

        var uploadResult = await response.Content.ReadFromJsonAsync<FileUploadResult>();

        return uploadResult.SomeNotNull(FileErrorCodes.RemoteStorageFailure_Download);
    }

    private void AddToContent(IBrowserFile file, FileType type, MultipartFormDataContent content, List<FileUploadResult> uploadResults, ref bool upload)
    {
        if (!FileValidations.ValidateFile(file, type, uploadResults)) return;

        StreamContent fileContent;
        try
        {
            fileContent = new StreamContent(file.OpenReadStream(FileValidations.allowedMaxFileSizeBytes));
        }
        catch (IOException ex)
        {
            Log.Error("{FileName} not uploaded: {Error}", file.Name, ex.Message);
            _snackbar.Add($"File {file.Name} may be too large. The max allowed size is 100MB.");

            uploadResults.Add(
                new FileUploadResult(
                    File: new FileDTO(Guid.Empty, file.Name, string.Empty, type, file.LastModified, file.ContentType),
                    Uploaded: false,
                    ErrorCode: FileErrorCodes.FileTooLarge
                ));

            return;
        }

        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        content.Add(
            content: fileContent,
            name: "\"files\"",
            fileName: file.Name);

        upload = true;
    }

    public async Task DownloadFileAsync(Guid dealId, FileDTO file)
    {
        var response = await _http.GetAsync($"api/v1/deals/{dealId}/file/{file.Id}");

        if (response.IsSuccessStatusCode)
            await DownloadFileAsync(await response.Content.ReadAsByteArrayAsync(), file.FileName);
        else
            await response.NotifyUserOfErrorsAsync(_snackbar);
    }

    public async Task DownloadTCsAsync(FileDTO file)
    {
        var response = await _http.GetAsync($"api/v1/company/tcs/download");

        if (response.IsSuccessStatusCode)
            await DownloadFileAsync(await response.Content.ReadAsByteArrayAsync(), file.FileName);
        else
            await response.NotifyUserOfErrorsAsync(_snackbar);
    }

    public async Task DownloadFileAsync(byte[] bytes, string fileName)
    {
        using var streamRef = new DotNetStreamReference(new MemoryStream(bytes));

        await _js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}

