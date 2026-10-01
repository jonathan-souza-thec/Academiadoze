using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoAlunoService
{
    Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<AcessoAlunoDto> AdicionarAsync(AcessoAlunoDto acessoDto, CancellationToken cancellationToken = default);
    Task<AcessoAlunoDto> AtualizarAsync(AcessoAlunoDto acessoDto, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
}