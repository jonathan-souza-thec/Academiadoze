namespace AcademiaDoZe.Presentation.AppMaui.Extensions;

public static class ShellExtensions
{
    public static Task DisplayAlertAsync(
        this Shell shell,
        string title,
        string message,
        string cancel)
        => shell.CurrentPage.DisplayAlertAsync(
            title,
            message,
            cancel);

    public static Task<bool> DisplayAlertAsync(
        this Shell shell,
        string title,
        string message,
        string accept,
        string cancel)
        => shell.CurrentPage.DisplayAlertAsync(
            title,
            message,
            accept,
            cancel);
}