// Jonathan de Souza Pereira

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
using System.Text;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    private const int TipoColaborador = 1;

    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    private const string BaseSelectQuery =
        "SELECT id_acesso, pessoa_id, data_hora, data_hora_saida FROM tb_acesso WHERE pessoa_tipo = 1";

    private static AcessoColaborador Map(DbDataReader reader) =>
        AcessoColaborador.Reconstituir(
            reader.GetInt32Value("id_acesso"),
            reader.GetInt32Value("pessoa_id"),
            reader.GetDateTimeValue("data_hora"),
            reader.GetNullableDateTime("data_hora_saida"));

    private static async Task<List<AcessoColaborador>> LerTodosAsync(DbCommand command, CancellationToken ct)
    {
        await using var reader = await command.ExecuteReaderAsync(ct);
        var lista = new List<AcessoColaborador>();
        while (await reader.ReadAsync(ct))
            lista.Add(Map(reader));
        return lista;
    }

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
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
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso de colaborador ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var command = await CreateCommandAsync($"{BaseSelectQuery} ORDER BY data_hora DESC", cancellationToken);
            return await LerTodosAsync(command, cancellationToken);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter acessos de colaboradores: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = DbProvider.FormatInsertQuery(
                "INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora, data_hora_saida) VALUES (@Tipo, @PessoaId, @DataHora, @Saida)",
                _databaseType);

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Tipo", TipoColaborador, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            command.AddParameter("@Saida", entity.DataHoraSaida, DbType.DateTime);

            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO", "Falha ao obter ID inserido para o acesso.", cancellationToken);
            typeof(Entity).GetProperty("Id")?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO", $"Erro ao adicionar acesso do colaborador ID {entity.ColaboradorId}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query =
                "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora, data_hora_saida = @Saida WHERE id_acesso = @Id AND pessoa_tipo = 1";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
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
            await using var command = await CreateCommandAsync("DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 1", cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO", $"Erro ao remover acesso ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(
        int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sb = new StringBuilder(BaseSelectQuery);
            if (colaboradorId.HasValue) sb.Append(" AND pessoa_id = @ColaboradorId");
            if (inicio.HasValue) sb.Append(" AND data_hora >= @Inicio");
            if (fim.HasValue) sb.Append(" AND data_hora < @Fim");
            sb.Append(" ORDER BY data_hora DESC");

            await using var command = await CreateCommandAsync(sb.ToString(), cancellationToken);
            if (colaboradorId.HasValue) command.AddParameter("@ColaboradorId", colaboradorId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            if (fim.HasValue) command.AddParameter("@Fim", fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);

            return await LerTodosAsync(command, cancellationToken);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_PERIODO", $"Erro ao obter acessos por período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND pessoa_id = @ColaboradorId ORDER BY data_hora DESC, id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ULTIMO_ACESSO", $"Erro ao obter último acesso do colaborador ID {colaboradorId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var ultimo = await ObterUltimoAcesso(colaboradorId, cancellationToken);
        return ultimo is { EstaDentro: true };
    }

    // Soma TODOS os registros do dia. Registro ainda aberto conta até o momento atual
    // (ou até o fim do dia, se for de um dia anterior).
    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        try
        {
            var inicio = data.ToDateTime(TimeOnly.MinValue);
            var fim = inicio.AddDays(1);

            string query = $"{BaseSelectQuery} AND pessoa_id = @ColaboradorId AND data_hora >= @Inicio AND data_hora < @Fim";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            command.AddParameter("@Inicio", inicio, DbType.DateTime);
            command.AddParameter("@Fim", fim, DbType.DateTime);

            var registros = await LerTodosAsync(command, cancellationToken);

            var agora = DateTime.Now;
            var total = TimeSpan.Zero;
            foreach (var r in registros)
            {
                var saida = r.DataHoraSaida ?? (agora < fim ? agora : fim);
                if (saida > r.DataHora)
                    total += saida - r.DataHora;
            }
            return total;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_HORAS_DIA", $"Erro ao calcular horas do colaborador ID {colaboradorId}: {ex.Message}", ex);
        }
    }
}