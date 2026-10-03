// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoColaboradorService
{
    Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<AcessoColaboradorDto> RegistrarEntradaAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<AcessoColaboradorDto> RegistrarSaidaAsync(int acessoId, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaboradorDto>> ObterPorColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<TimeSpan> ObterTotalHorasTrabalhadasHojeAsync(int colaboradorId, CancellationToken cancellationToken = default);
}