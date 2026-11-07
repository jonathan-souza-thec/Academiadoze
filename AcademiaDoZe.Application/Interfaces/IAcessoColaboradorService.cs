// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoColaboradorService
{
    Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    // Valida o limite diário (8h CLT / 6h estágio) e registra a entrada. Lança AcessoNegadoException se não autorizado.
    Task<AcessoColaboradorDto> RegistrarEntradaAsync(int colaboradorId, CancellationToken cancellationToken = default);
    // Registra a saída e informa a permanência, somando todos os registros do dia.
    Task<AcessoColaboradorDto> RegistrarSaidaAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaboradorDto>> ObterPorColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<TimeSpan> ObterTotalHorasTrabalhadasHojeAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<bool> EstaNaAcademiaAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaboradorDto>> ObterDoDiaAsync(CancellationToken cancellationToken = default);
}