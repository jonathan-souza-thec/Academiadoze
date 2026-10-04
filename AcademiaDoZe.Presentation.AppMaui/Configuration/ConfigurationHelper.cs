using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var (connectionString, databaseType) = ObterConfiguracaoAtual();

        var repoConfig = new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        };

        // Configura a fábrica de repositórios com a string de conexão e tipo de banco
        services.AddSingleton(repoConfig);

        // Atualiza a configuração quando o usuário alterar o banco de dados
        WeakReferenceMessenger.Default.Register<RepositoryConfig, BancoPreferencesUpdatedMessage>(
            repoConfig,
            (r, m) =>
            {
                var (novaConnStr, novoDbType) = ObterConfiguracaoAtual();

                r.ConnectionString = novaConnStr;
                r.DatabaseType = novoDbType.ToInfrastructure();
            });

        // Configura os serviços da camada de aplicação
        services.AddApplicationServices();
    }

    /// <summary>
    /// Obtém a Connection String e o AppDatabaseType ativos
    /// a partir das Preferences do usuário.
    /// </summary>
    public static (string ConnectionString, AppDatabaseType DatabaseType) ObterConfiguracaoAtual()
    {
        // Padrão: MySQL
        var databaseTypeStr = Preferences.Get(
            "DatabaseType",
            AppDatabaseType.MySql.ToString());

        if (!Enum.TryParse<AppDatabaseType>(
                databaseTypeStr,
                out var databaseType))
        {
            databaseType = AppDatabaseType.MySql;
        }

        string connectionString;

        // =========================================================
        // SQLITE
        // =========================================================

        if (databaseType == AppDatabaseType.Sqlite)
        {
            var defaultDbPath = DeviceInfo.Platform == DevicePlatform.WinUI
                ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
                : Path.Combine(
                    FileSystem.AppDataDirectory,
                    "db_academia_do_ze.db");

            var dbPath = Preferences.Get(
                "Sqlite_Caminho",
                defaultDbPath);

            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = defaultDbPath;
            }

            var complemento = Preferences.Get(
                "Sqlite_Complemento",
                "Default Timeout=5;");

            connectionString =
                $"Data Source={dbPath};{complemento}";
        }

        // =========================================================
        // MYSQL / SQL SERVER
        // =========================================================

        else
        {
            var prefix = databaseType == AppDatabaseType.SqlServer
                ? "SqlServer"
                : "MySql";

            // -----------------------------------------------------
            // SERVIDOR
            // -----------------------------------------------------

            string defaultServer;

            if (databaseType == AppDatabaseType.SqlServer)
            {
                // SQL Server
                defaultServer = "172.24.32.1";
            }
            else
            {
                // MySQL
                //
                // No Android Emulator:
                // 10.0.2.2 = computador Windows
                //
                // No Windows:
                // localhost = computador local

                defaultServer = DeviceInfo.Platform == DevicePlatform.Android
                    ? "10.0.2.2"
                    : "localhost";
            }

            // -----------------------------------------------------
            // USUÁRIO
            // -----------------------------------------------------

            var defaultUser = databaseType == AppDatabaseType.SqlServer
                ? "sa"
                : "root";

            // -----------------------------------------------------
            // COMPLEMENTO
            // -----------------------------------------------------

            var defaultComplemento = databaseType == AppDatabaseType.SqlServer
                ? "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;"
                : "Port=3306;Connection Timeout=5;Default Command Timeout=30;";

            // -----------------------------------------------------
            // PREFERENCES
            // -----------------------------------------------------

            var dbServer = Preferences.Get(
                $"{prefix}_Servidor",
                defaultServer);

            var dbDatabase = Preferences.Get(
                $"{prefix}_Banco",
                "db_academia_do_ze");

            var dbUser = Preferences.Get(
                $"{prefix}_Usuario",
                defaultUser);

            var dbPassword = Preferences.Get(
                $"{prefix}_Senha",
                "abcBolinhas12345");

            var dbComplemento = Preferences.Get(
                $"{prefix}_Complemento",
                defaultComplemento);

            // -----------------------------------------------------
            // CONNECTION STRING
            // -----------------------------------------------------

            connectionString =
                $"Server={dbServer};" +
                $"Port={(databaseType == AppDatabaseType.MySql ? "3306;" : "")}" +
                $"Database={dbDatabase};" +
                $"User Id={dbUser};" +
                $"Password={dbPassword};" +
                $"{dbComplemento}";
        }

        return (connectionString, databaseType);
    }
}