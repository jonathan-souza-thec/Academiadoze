using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _repoFactory;

    public AcessoAlunoService(Func<IAcessoAlunoRepository> repoFactory)
    {
        _repoFactory = repoFactory;
    }

    public Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoAlunoDto> AdicionarAsync(AcessoAlunoDto acessoDto, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoAlunoDto> AtualizarAsync(AcessoAlunoDto acessoDto, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}