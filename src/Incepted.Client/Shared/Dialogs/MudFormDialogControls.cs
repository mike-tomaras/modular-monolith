using MudBlazor;

namespace Incepted.Client.Shared.Dialogs;

public class MudFormDialogControls
{
    public bool IsVisible { get; set; }
    public bool ValidationSuccess { get; set; }
    public MudForm? Form { get; set; }
    public bool Processing { get; set; }
    public DialogOptions DefaultDialogOptions = new() { FullWidth = true, CloseButton = true, CloseOnEscapeKey = true };
    public void OpenDialog() => IsVisible = true;
}
