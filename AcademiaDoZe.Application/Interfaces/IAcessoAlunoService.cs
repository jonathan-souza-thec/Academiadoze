// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoAlunoService
{
    Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<AcessoAlunoDto> RegistrarEntradaAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<AcessoAlunoDto> RegistrarSaidaAsync(int acessoId, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAlunoDto>> ObterPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    // Acesso com entrada registrada e saída pendente (aluno está dentro da academia)
    Task<AcessoAlunoDto?> ObterAcessoAbertoAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAlunoDto>> ObterDeHojeAsync(CancellationToken cancellationToken = default);
}