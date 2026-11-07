// Jonathan de Souza Pereira

using AcademiaDoZe.Infrastructure.Exceptions;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;

namespace AcademiaDoZe.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly ConcurrentDictionary<string, bool>
        _bancosInicializados = new();

    public static async Task InicializarAsync(
        string connectionString,
        DatabaseType databaseType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InfrastructureException(
                "CONEXAO_STRING_VAZIA",
                "String de conexão não pode ser vazia.");

        var key = $"{databaseType}:{connectionString}";

        if (_bancosInicializados.ContainsKey(key))
            return;

        try
        {
            var scriptSql = ObterScript(databaseType);

            await using var connection =
                DbProvider.CreateConnection(
                    connectionString,
                    databaseType);

            await connection.OpenAsync(cancellationToken);

            await using var command =
                DbProvider.CreateCommand(
                    scriptSql,
                    connection);

            command.CommandTimeout =
                DbProvider.DefaultCommandTimeout;

            await command.ExecuteNonQueryAsync(
                cancellationToken);

            // Migração: garante a coluna de saída em bancos criados antes da Avaliação 03
            await GarantirColunaSaidaAcessoAsync(
                connectionString, databaseType, cancellationToken);

            _bancosInicializados.TryAdd(key, true);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "ERRO_INICIALIZAR_BANCO",
                $"Erro ao inicializar banco de dados " +
                $"({databaseType}): {ex.Message}",
                ex);
        }
        catch (InfrastructureException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException(
                "ERRO_INICIALIZAR_BANCO",
                $"Erro inesperado ao inicializar banco de dados " +
                $"({databaseType}): {ex.Message}",
                ex);
        }
    }

    // Se a coluna já existir, o banco devolve erro e simplesmente ignoramos.
    private static async Task GarantirColunaSaidaAcessoAsync(
        string connectionString,
        DatabaseType databaseType,
        CancellationToken cancellationToken)
    {
        var sql = databaseType switch
        {
            DatabaseType.SqlServer => "ALTER TABLE tb_acesso ADD data_hora_saida DATETIME NULL",
            DatabaseType.MySql => "ALTER TABLE tb_acesso ADD COLUMN data_hora_saida DATETIME NULL",
            _ => "ALTER TABLE tb_acesso ADD COLUMN data_hora_saida TEXT NULL"
        };

        try
        {
            await using var connection =
                DbProvider.CreateConnection(connectionString, databaseType);
            await connection.OpenAsync(cancellationToken);

            await using var command = DbProvider.CreateCommand(sql, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (DbException)
        {
            // coluna já existe: nada a fazer
        }
    }

    public static string ObterScript(
        DatabaseType databaseType)
    {
        var nomeScript =
            DbProvider.GetScriptName(databaseType);

        var assembly =
            Assembly.GetExecutingAssembly();

        var resourceName =
            assembly
                .GetManifestResourceNames()
                .FirstOrDefault(r =>
                    r.EndsWith(
                        nomeScript,
                        StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
        {
            throw new InfrastructureException(
                "SCRIPT_EMBARCADO_NAO_ENCONTRADO",
                $"Script SQL embarcado '{nomeScript}' não encontrado.");
        }

        using var stream =
            assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            throw new InfrastructureException(
                "ERRO_LEITURA_SCRIPT",
                $"Erro ao carregar o script '{nomeScript}'.");
        }

        using var reader =
            new StreamReader(stream);

        return reader.ReadToEnd();
    }
}