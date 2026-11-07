// Jonathan de Souza Pereira
namespace AcademiaDoZe.Application.DTOs;

public class AcessoColaboradorDto
{
    public int Id { get; set; }
    public ColaboradorDto? Colaborador { get; set; }
    public DateTime Entrada { get; set; }
    public DateTime? Saida { get; set; }
    public TimeSpan? TempoPermanencia => Saida.HasValue ? Saida.Value - Entrada : null;

    // Resultado da operação (usado pela tela da catraca)
    public bool Autorizado { get; set; } = true;
    public string Mensagem { get; set; } = string.Empty;
    public TimeSpan? TotalHorasDia { get; set; }
}