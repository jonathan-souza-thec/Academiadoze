namespace AcademiaDoZe.Presentation.AppMaui.Extensions;

// Se o projeto já tiver uma extensão equivalente, apague este arquivo (evita CS0111).
public static class ShellExtensions
{
    public static Task DisplayAlertAsync(this Shell shell, string title, string message, string cancel)
        => shell.DisplayAlert(title, message, cancel);

    public static Task<bool> DisplayAlertAsync(this Shell shell, string title, string message, string accept, string cancel)
        => shell.DisplayAlert(title, message, accept, cancel);
}