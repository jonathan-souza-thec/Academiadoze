// Jonathan de Souza Pereira
namespace AcademiaDoZe.Application.Common;

public static class TimeSpanExtensions
{
    // Formata como HH:mm:ss (horas podem passar de 24)
    public static string ToHoraMinutoSegundo(this TimeSpan tempo)
    {
        if (tempo < TimeSpan.Zero) tempo = TimeSpan.Zero;
        return $"{(int)tempo.TotalHours:00}:{tempo.Minutes:00}:{tempo.Seconds:00}";
    }
}