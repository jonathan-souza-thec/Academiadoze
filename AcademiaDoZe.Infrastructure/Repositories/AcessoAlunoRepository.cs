using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : IAcessoAlunoRepository
{
    private readonly string _connectionString;
    private readonly object _databaseType;

    public AcessoAlunoRepository(string connectionString, object databaseType)
    {
        _connectionString = connectionString;
        _databaseType = databaseType;
    }

    public Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> Remover(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}