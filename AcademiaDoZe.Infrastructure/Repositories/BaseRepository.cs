// Jonathan de Souza Pereira

using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public abstract class BaseRepository : IDisposable, IAsyncDisposable
{
    protected readonly string _connectionString;
    protected readonly DatabaseType _databaseType;

    private DbConnection? _connection;
    private bool _disposed;

    protected BaseRepository(
        string connectionString,
        DatabaseType databaseType)
    {
        _connectionString =
            connectionString
            ?? throw new InfrastructureException(
                "STRING_CONEXAO_NULA",
                $"String de conexão não pode ser nula: {nameof(connectionString)}");

        _databaseType = databaseType;
    }

    protected virtual async Task<DbConnection> GetOpenConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            // Inicializa o banco e cria as tabelas,
            // caso ainda não existam.
            await DbInitializer.InicializarAsync(
                _connectionString,
                _databaseType,
                cancellationToken);

            if (_connection == null)
            {
                _connection = DbProvider.CreateConnection(
                    _connectionString,
                    _databaseType);

                await _connection.OpenAsync(cancellationToken);
            }
            else if (_connection.State == ConnectionState.Broken)
            {
                await _connection.CloseAsync();

                await _connection.OpenAsync(cancellationToken);
            }
            else if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync(cancellationToken);
            }

            return _connection;
        }
        catch (InfrastructureException)
        {
            // Mantém a mensagem original da infraestrutura.
            throw;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException(
                "FALHA_ABRIR_CONEXAO",
                $"Falha ao abrir conexão com o banco de dados: {ex.Message}",
                ex);
        }
        catch (Exception ex)
        {
            throw new InfrastructureException(
                "FALHA_ABRIR_CONEXAO",
                $"Erro inesperado ao abrir conexão com o banco de dados: {ex.Message}",
                ex);
        }
    }

    protected virtual async Task<DbCommand> CreateCommandAsync(
        string commandText,
        CancellationToken cancellationToken = default)
    {
        var connection =
            await GetOpenConnectionAsync(cancellationToken);

        return DbProvider.CreateCommand(
            commandText,
            connection);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }

        _disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();

        Dispose(false);

        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
            return;

        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}