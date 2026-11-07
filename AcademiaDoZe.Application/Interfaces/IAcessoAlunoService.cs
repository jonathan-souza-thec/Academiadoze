// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoAlunoService
{
    Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    // Valida matrícula ativa e registra a entrada. Lança AcessoNegadoException se não autorizado.
    Task<AcessoAlunoDto> RegistrarEntradaAsync(int alunoId, CancellationToken cancellationToken = default);
    // Registra a saída da entrada em aberto do aluno e informa o tempo de permanência.
    Task<AcessoAlunoDto> RegistrarSaidaAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAlunoDto>> ObterPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<bool> EstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoAlunoDto>> ObterDoDiaAsync(CancellationToken cancellationToken = default);
}