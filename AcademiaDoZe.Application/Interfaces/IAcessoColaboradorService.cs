using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoColaboradorService
{
    Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<AcessoColaboradorDto> AdicionarAsync(AcessoColaboradorDto acessoDto, CancellationToken cancellationToken = default);
    Task<AcessoColaboradorDto> AtualizarAsync(AcessoColaboradorDto acessoDto, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}