// Jonathan de Souza Pereira

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
using System.Text;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    private const int TipoAluno = 0;

    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    private const string BaseSelectQuery =
        "SELECT id_acesso, pessoa_id, data_hora, data_hora_saida FROM tb_acesso WHERE pessoa_tipo = 0";

    private static AcessoAluno Map(DbDataReader reader) =>
        AcessoAluno.Reconstituir(
            reader.GetInt32Value("id_acesso"),
            reader.GetInt32Value("pessoa_id"),
            reader.GetDateTimeValue("data_hora"),
            reader.GetNullableDateTime("data_hora_saida"));

    private static async Task<List<AcessoAluno>> LerTodosAsync(DbCommand command, CancellationToken ct)
    {
        await using var reader = await command.ExecuteReaderAsync(ct);
        var lista = new List<AcessoAluno>();
        while (await reader.ReadAsync(ct))
            lista.Add(Map(reader));
        return lista;
    }

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync($"{BaseSelectQuery} AND id_acesso = @Id", cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso de aluno ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync($"{BaseSelectQuery} ORDER BY data_hora DESC", cancellationToken);
            return await LerTodosAsync(command, cancellationToken);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter acessos de alunos: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = DbProvider.FormatInsertQuery(
                "INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora, data_hora_saida) VALUES (@Tipo, @PessoaId, @DataHora, @Saida)",
                _databaseType);

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Tipo", TipoAluno, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            command.AddParameter("@Saida", entity.DataHoraSaida, DbType.DateTime);

            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO", "Falha ao obter ID inserido para o acesso.", cancellationToken);
            typeof(Entity).GetProperty("Id")?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO", $"Erro ao adicionar acesso do aluno ID {entity.AlunoId}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query =
                "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora, data_hora_saida = @Saida WHERE id_acesso = @Id AND pessoa_tipo = 0";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            command.AddParameter("@Saida", entity.DataHoraSaida, DbType.DateTime);

            int linhas = await command.ExecuteNonQueryAsync(cancellationToken);
            if (linhas == 0)
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso encontrado com o ID {entity.Id} para atualização.");
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO", $"Erro ao atualizar acesso ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync("DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 0", cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO", $"Erro ao remover acesso ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(
        int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sb = new StringBuilder(BaseSelectQuery);
            if (alunoId.HasValue) sb.Append(" AND pessoa_id = @AlunoId");
            if (inicio.HasValue) sb.Append(" AND data_hora >= @Inicio");
            if (fim.HasValue) sb.Append(" AND data_hora < @Fim");
            sb.Append(" ORDER BY data_hora DESC");

            await using var command = await CreateCommandAsync(sb.ToString(), cancellationToken);
            if (alunoId.HasValue) command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            if (fim.HasValue) command.AddParameter("@Fim", fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);

            return await LerTodosAsync(command, cancellationToken);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_PERIODO", $"Erro ao obter acessos por período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND pessoa_id = @AlunoId ORDER BY data_hora DESC, id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ULTIMO_ACESSO", $"Erro ao obter último acesso do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        var ultimo = await ObterUltimoAcesso(alunoId, cancellationToken);
        return ultimo is { EstaDentro: true };
    }

    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            string hora = DbProvider.GetDateHourExpression("data_hora", _databaseType);
            string mesExpr = DbProvider.GetDateMonthExpression("data_hora", _databaseType);
            string query =
                $"SELECT {hora} AS hora, COUNT(*) AS total FROM tb_acesso WHERE pessoa_tipo = 0 AND {mesExpr} = @Mes GROUP BY {hora} ORDER BY total DESC";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Mes", mes, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var resultado = new Dictionary<TimeOnly, int>();
            while (await reader.ReadAsync(cancellationToken))
                resultado[new TimeOnly(Convert.ToInt32(reader["hora"]), 0)] = Convert.ToInt32(reader["total"]);
            return resultado;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_HORARIO_PROCURADO", $"Erro ao obter horário mais procurado: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            string mesExpr = DbProvider.GetDateMonthExpression("data_hora", _databaseType);
            string query = $"{BaseSelectQuery} AND data_hora_saida IS NOT NULL AND {mesExpr} = @Mes";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Mes", mes, DbType.Int32);
            var acessos = await LerTodosAsync(command, cancellationToken);

            var resultado = new Dictionary<int, TimeSpan>();
            if (acessos.Count > 0)
                resultado[mes] = TimeSpan.FromTicks((long)acessos.Average(a => a.TempoPermanencia!.Value.Ticks));
            return resultado;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_PERMANENCIA_MEDIA", $"Erro ao obter permanência média: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            var comAcesso = new HashSet<int>();
            await using (var command = await CreateCommandAsync(
                "SELECT DISTINCT pessoa_id FROM tb_acesso WHERE pessoa_tipo = 0 AND data_hora >= @Limite", cancellationToken))
            {
                command.AddParameter("@Limite", DateTime.Now.AddDays(-dias), DbType.DateTime);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    comAcesso.Add(Convert.ToInt32(reader["pessoa_id"]));
            }

            await using var alunoRepo = new AlunoRepository(_connectionString, _databaseType);
            var todos = await alunoRepo.ObterTodos(cancellationToken);
            return todos.Where(a => !comAcesso.Contains(a.Id)).ToList();
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ALUNOS_SEM_ACESSO", $"Erro ao obter alunos sem acesso: {ex.Message}", ex);
        }
    }
}