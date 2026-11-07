// Jonathan de Souza Pereira
namespace AcademiaDoZe.Application.DTOs;

// Representa uma "ida à academia": entrada e (quando houver) saída.
public class AcessoColaboradorDto
{
    public int Id { get; set; }
    public ColaboradorDto? Colaborador { get; set; }
    public DateTime Entrada { get; set; }
    public DateTime? Saida { get; set; }
    public TimeSpan? TempoPermanencia => Saida.HasValue ? Saida.Value - Entrada : null;

    // Preenchidos ao registrar entrada/saída (feedback para o usuário)
    public TimeSpan? TotalHorasHoje { get; set; }
    public string? Mensagem { get; set; }
}