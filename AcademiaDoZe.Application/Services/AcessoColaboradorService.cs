using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository> _repoFactory;

    public AcessoColaboradorService(Func<IAcessoColaboradorRepository> repoFactory)
    {
        _repoFactory = repoFactory;
    }

    public Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoColaboradorDto> AdicionarAsync(AcessoColaboradorDto acessoDto, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoColaboradorDto> AtualizarAsync(AcessoColaboradorDto acessoDto, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}