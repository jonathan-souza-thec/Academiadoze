// Jonathan de Souza Pereira
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

// tb_acesso guarda um registro (data/hora) por passagem na catraca:
// pessoa_tipo = 1 (Colaborador). Dentro do dia, 1º registro = entrada, 2º = saída, 3º = entrada...
public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    private static string BaseSelectQuery => """
SELECT
ac.id_acesso, ac.data_hora,
c.id_colaborador, c.cpf, c.nome AS colaborador_nome, c.nascimento, c.telefone, c.email, c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo,
l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
FROM tb_acesso ac
INNER JOIN tb_colaborador c ON ac.pessoa_id = c.id_colaborador
INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro
WHERE ac.pessoa_tipo = 1
""";

    public static AcessoColaborador Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_acesso");
            DateTime dataHora = reader.GetDateTimeValue("data_hora");
            var colaborador = ColaboradorRepository.Map(reader, "colaborador_nome");
            var result = AcessoColaborador.Criar(id, colaborador, dataHora);
            if (result.IsFailure)
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear acesso ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_COLABORADOR", $"Erro ao mapear dados do acesso do colaborador: {ex.Message}", ex);
        }
    }

    private static DateTime InicioDoDia(DateOnly data) => data.ToDateTime(TimeOnly.MinValue);

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND ac.id_acesso = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY ac.data_hora, ac.id_acesso";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
                lista.Add(Map(reader));
            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os acessos de colaboradores: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = DbProvider.FormatInsertQuery(
                "INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (1, @PessoaId, @DataHora)",
                _databaseType);
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            int id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO", "Falha ao obter ID inserido para o acesso.", cancellationToken);
            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO", $"Erro ao adicionar acesso do colaborador: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = 1";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            int linhas = await command.ExecuteNonQueryAsync(cancellationToken);
            if (linhas == 0)
                throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO", $"Acesso ID {entity.Id} não encontrado para atualização.");
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO", $"Erro ao atualizar acesso do colaborador: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 1";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO", $"Erro ao remover acesso ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filtros = new List<string>();
            if (colaboradorId.HasValue) filtros.Add("ac.pessoa_id = @ColaboradorId");
            if (inicio.HasValue) filtros.Add("ac.data_hora >= @Inicio");
            if (fim.HasValue) filtros.Add("ac.data_hora < @FimExclusivo");

            string query = BaseSelectQuery;
            if (filtros.Count > 0) query += " AND " + string.Join(" AND ", filtros);
            query += " ORDER BY ac.data_hora, ac.id_acesso";

            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (colaboradorId.HasValue) command.AddParameter("@ColaboradorId", colaboradorId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", InicioDoDia(inicio.Value), DbType.DateTime);
            if (fim.HasValue) command.AddParameter("@FimExclusivo", InicioDoDia(fim.Value).AddDays(1), DbType.DateTime);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
                lista.Add(Map(reader));
            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_PERIODO", $"Erro ao obter acessos do período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND ac.pessoa_id = @ColaboradorId ORDER BY ac.data_hora DESC, ac.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO", $"Erro ao obter último acesso do colaborador ID {colaboradorId}: {ex.Message}", ex);
        }
    }

    // Está na academia quando o nº de registros de HOJE é ímpar (entrada sem saída correspondente)
    public async Task<bool> EstaNaAcademia(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM tb_acesso WHERE pessoa_tipo = 1 AND pessoa_id = @ColaboradorId AND data_hora >= @Inicio AND data_hora < @Fim";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            var hoje = DateTime.Today;
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            command.AddParameter("@Inicio", hoje, DbType.DateTime);
            command.AddParameter("@Fim", hoje.AddDays(1), DbType.DateTime);
            var resultado = await command.ExecuteScalarAsync(cancellationToken);
            int total = resultado == null || resultado == DBNull.Value ? 0 : Convert.ToInt32(resultado);
            return total % 2 == 1;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ESTA_NA_ACADEMIA", $"Erro ao verificar presença do colaborador ID {colaboradorId}: {ex.Message}", ex);
        }
    }

    // Soma as permanências do dia (pares entrada/saída). Se houver entrada em aberto hoje, conta até o momento atual.
    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        var acessos = (await ObterAcessosPorColaboradorPeriodo(colaboradorId, data, data, cancellationToken))
            .OrderBy(a => a.DataHora)
            .ThenBy(a => a.Id)
            .ToList();

        var total = TimeSpan.Zero;
        for (int i = 0; i < acessos.Count; i += 2)
        {
            if (i + 1 < acessos.Count)
                total += acessos[i + 1].DataHora - acessos[i].DataHora;
            else if (data == DateOnly.FromDateTime(DateTime.Today))
                total += DateTime.Now - acessos[i].DataHora;
        }
        return total < TimeSpan.Zero ? TimeSpan.Zero : total;
    }
}