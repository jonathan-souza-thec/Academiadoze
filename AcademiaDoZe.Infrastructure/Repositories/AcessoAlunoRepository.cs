
// Jonathan de Souza Pereira

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    public AcessoAlunoRepository(
        string connectionString,
        DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    // ============================================================
    // MAPEAMENTO
    // ============================================================

    private static AcessoAluno Map(DbDataReader reader)
    {
        int id = reader.GetInt32Value("id_acesso");
        int alunoId = reader.GetInt32Value("pessoa_id");
        DateTime entrada = reader.GetDateTimeValue("data_hora");

        DateTime? saida = reader["data_saida"] == DBNull.Value
            ? null
            : Convert.ToDateTime(reader["data_saida"]);

        return AcessoAluno.Reconstituir(
            id,
            alunoId,
            entrada,
            saida);
    }

    // ============================================================
    // CONSULTA POR ID
    // ============================================================

    public async Task<AcessoAluno?> ObterPorId(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id_acesso, pessoa_id, data_hora, data_saida
            FROM tb_acesso
            WHERE id_acesso = @Id
              AND pessoa_tipo = 0
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter("@Id", id, DbType.Int32);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? Map(reader)
            : null;
    }

    // ============================================================
    // CONSULTAR TODOS OS ACESSOS
    // ============================================================

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id_acesso, pessoa_id, data_hora, data_saida
            FROM tb_acesso
            WHERE pessoa_tipo = 0
            ORDER BY data_hora DESC
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var acessos = new List<AcessoAluno>();

        while (await reader.ReadAsync(cancellationToken))
            acessos.Add(Map(reader));

        return acessos;
    }

    // ============================================================
    // ADICIONAR ACESSO
    // ============================================================

    public async Task<AcessoAluno> Adicionar(
        AcessoAluno entity,
        CancellationToken cancellationToken = default)
    {
        string sql = DbProvider.FormatInsertQuery(
            """
            INSERT INTO tb_acesso
                (pessoa_tipo, pessoa_id, data_hora, data_saida)
            VALUES
                (0, @AlunoId, @Entrada, @Saida)
            """,
            _databaseType);

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter(
            "@AlunoId",
            entity.AlunoId,
            DbType.Int32);

        command.AddParameter(
            "@Entrada",
            entity.DataHora,
            DbType.DateTime);

        command.AddParameter(
            "@Saida",
            (object?)entity.Saida ?? DBNull.Value,
            DbType.DateTime);

        int id = await command.ExecuteScalarIdAsync(
            "ERRO_ADICIONAR_ACESSO_ALUNO",
            "Não foi possível obter o ID do acesso cadastrado.",
            cancellationToken);

        typeof(Entity)
            .GetProperty("Id")!
            .SetValue(entity, id);

        return entity;
    }

    // ============================================================
    // ATUALIZAR ACESSO
    // ============================================================

    public async Task<AcessoAluno> Atualizar(
        AcessoAluno entity,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE tb_acesso
            SET data_hora = @Entrada,
                data_saida = @Saida
            WHERE id_acesso = @Id
              AND pessoa_tipo = 0
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter("@Id", entity.Id, DbType.Int32);
        command.AddParameter("@Entrada", entity.DataHora, DbType.DateTime);
        command.AddParameter(
            "@Saida",
            (object?)entity.Saida ?? DBNull.Value,
            DbType.DateTime);

        int linhas = await command.ExecuteNonQueryAsync(cancellationToken);

        if (linhas == 0)
        {
            throw new InfrastructureException(
                "ACESSO_ALUNO_NAO_ENCONTRADO",
                $"Acesso do aluno com ID {entity.Id} não encontrado.");
        }

        return entity;
    }

    // ============================================================
    // REMOVER ACESSO
    // ============================================================

    public async Task<bool> Remover(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            DELETE FROM tb_acesso
            WHERE id_acesso = @Id
              AND pessoa_tipo = 0
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter("@Id", id, DbType.Int32);

        int linhas = await command.ExecuteNonQueryAsync(cancellationToken);

        return linhas > 0;
    }

    // ============================================================
    // ACESSOS POR ALUNO E PERÍODO
    // ============================================================

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(
        int? alunoId = null,
        DateOnly? inicio = null,
        DateOnly? fim = null,
        CancellationToken cancellationToken = default)
    {
        string sql = """
            SELECT id_acesso, pessoa_id, data_hora, data_saida
            FROM tb_acesso
            WHERE pessoa_tipo = 0
            """;

        if (alunoId.HasValue)
            sql += " AND pessoa_id = @AlunoId";

        if (inicio.HasValue)
            sql += " AND data_hora >= @Inicio";

        if (fim.HasValue)
            sql += " AND data_hora < @Fim";

        sql += " ORDER BY data_hora DESC";

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        if (alunoId.HasValue)
            command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);

        if (inicio.HasValue)
        {
            command.AddParameter(
                "@Inicio",
                inicio.Value.ToDateTime(TimeOnly.MinValue),
                DbType.DateTime);
        }

        if (fim.HasValue)
        {
            command.AddParameter(
                "@Fim",
                fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue),
                DbType.DateTime);
        }

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var acessos = new List<AcessoAluno>();

        while (await reader.ReadAsync(cancellationToken))
            acessos.Add(Map(reader));

        return acessos;
    }

    // ============================================================
    // ÚLTIMO ACESSO DO ALUNO
    // ============================================================

    public async Task<AcessoAluno?> ObterUltimoAcesso(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id_acesso, pessoa_id, data_hora, data_saida
            FROM tb_acesso
            WHERE pessoa_tipo = 0
              AND pessoa_id = @AlunoId
            ORDER BY data_hora DESC
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter("@AlunoId", alunoId, DbType.Int32);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? Map(reader)
            : null;
    }

    // ============================================================
    // VERIFICAR SE O ALUNO ESTÁ NA ACADEMIA
    // ============================================================

    public async Task<bool> EstaNaAcademia(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM tb_acesso
            WHERE pessoa_tipo = 0
              AND pessoa_id = @AlunoId
              AND data_saida IS NULL
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter("@AlunoId", alunoId, DbType.Int32);

        object? resultado =
            await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(resultado) > 0;
    }

    // ============================================================
    // HORÁRIO MAIS PROCURADO POR MÊS
    // ============================================================

    public async Task<Dictionary<TimeOnly, int>>
        ObterHorarioMaisProcuradoPorMes(
            int mes,
            CancellationToken cancellationToken = default)
    {
        if (mes < 1 || mes > 12)
            throw new ArgumentOutOfRangeException(
                nameof(mes),
                "O mês deve estar entre 1 e 12.");

        string hora = DbProvider.GetDateHourExpression(
            "data_hora",
            _databaseType);

        string sql = $"""
            SELECT {hora} AS hora, COUNT(*) AS quantidade
            FROM tb_acesso
            WHERE pessoa_tipo = 0
              AND {DbProvider.GetDateMonthExpression("data_hora", _databaseType)} = @Mes
            GROUP BY {hora}
            ORDER BY quantidade DESC
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter("@Mes", mes, DbType.Int32);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var resultado = new Dictionary<TimeOnly, int>();

        while (await reader.ReadAsync(cancellationToken))
        {
            int horaValor = reader.GetInt32Value("hora");
            int quantidade = reader.GetInt32Value("quantidade");

            resultado[new TimeOnly(horaValor, 0)] = quantidade;
        }

        return resultado;
    }

    // ============================================================
    // PERMANÊNCIA MÉDIA POR MÊS
    // ============================================================

    public async Task<Dictionary<int, TimeSpan>>
        ObterPermanenciaMediaPorMes(
            int mes,
            CancellationToken cancellationToken = default)
    {
        // O parâmetro mes representa o mês desejado (1 a 12).
        if (mes < 1 || mes > 12)
            throw new ArgumentOutOfRangeException(
                nameof(mes),
                "O mês deve estar entre 1 e 12.");

        // Calculamos a média em C# para manter compatibilidade
        // entre SQLite, MySQL e SQL Server.
        var acessos = await ObterAcessosPorAlunoPeriodo(
            null,
            null,
            null,
            cancellationToken);

        var permanencias = acessos
            .Where(a =>
                a.Saida.HasValue &&
                a.DataHora.Month == mes)
            .GroupBy(a => a.DataHora.Month);

        var resultado = new Dictionary<int, TimeSpan>();

        foreach (var grupo in permanencias)
        {
            var tempos = grupo
                .Select(a => a.Saida!.Value - a.DataHora)
                .ToList();

            if (tempos.Count > 0)
            {
                long ticks = (long)tempos.Average(t => t.Ticks);
                resultado[grupo.Key] = TimeSpan.FromTicks(ticks);
            }
        }

        return resultado;
    }

    // ============================================================
    // ALUNOS SEM ACESSO NOS ÚLTIMOS DIAS
    // ============================================================

    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(
        int dias,
        CancellationToken cancellationToken = default)
    {
        if (dias < 0)
            throw new ArgumentOutOfRangeException(
                nameof(dias),
                "A quantidade de dias não pode ser negativa.");

        string dataLimite = DbProvider.GetDateAddDaysExpression(
            DbProvider.GetCurrentDateFunction(_databaseType),
            "@Dias",
            _databaseType);

        string sql = $"""
            SELECT
                a.id_aluno,
                a.cpf,
                a.nome,
                a.nascimento,
                a.telefone,
                a.email,
                a.logradouro_id,
                a.numero,
                a.complemento,
                a.senha,
                a.foto,
                l.id_logradouro,
                l.cep,
                l.nome AS logradouro_nome,
                l.bairro,
                l.cidade,
                l.estado,
                l.pais
            FROM tb_aluno a
            INNER JOIN tb_logradouro l
                ON a.logradouro_id = l.id_logradouro
            WHERE NOT EXISTS (
                SELECT 1
                FROM tb_acesso ac
                WHERE ac.pessoa_tipo = 0
                  AND ac.pessoa_id = a.id_aluno
                  AND ac.data_hora >= {dataLimite}
            )
            ORDER BY a.nome
            """;

        await using var command =
            await CreateCommandAsync(sql, cancellationToken);

        command.AddParameter("@Dias", dias, DbType.Int32);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var alunos = new List<Aluno>();

        while (await reader.ReadAsync(cancellationToken))
            alunos.Add(AlunoRepository.Map(reader));

        return alunos;
    }
}
