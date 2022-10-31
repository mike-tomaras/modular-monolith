using Incepted.Shared.DTOs;
using MudBlazor;

namespace Incepted.Client.Extensions;

internal static class FileExtensions
{
    public static void NotifyOfErrors(IEnumerable<FileUploadResult> uploadResults, ISnackbar snackbar) =>
        uploadResults
            .Where(r => !r.Uploaded)
            .ToList()
            .ForEach(r => snackbar.Add(r.ErrorCode.errors.name.First(), Severity.Error));
}
