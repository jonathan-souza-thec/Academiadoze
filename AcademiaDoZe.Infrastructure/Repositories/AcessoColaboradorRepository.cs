using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : IAcessoColaboradorRepository
{
    private readonly string _connectionString;
    private readonly object _databaseType;

    public AcessoColaboradorRepository(string connectionString, object databaseType)
    {
        _connectionString = connectionString;
        _databaseType = databaseType;
    }

    public Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> Remover(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}