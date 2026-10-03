// Jonathan de Souza Pereira
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Application.Mappings;

public static class DatabaseTypeEnumMappingExtensions
{
    // CORREÇÃO: conversão por nome, e não por cast numérico.
    // AppDatabaseType usa SqlServer=0, Sqlite=1, MySql=2, enquanto o DatabaseType da
    // Infrastructure usa SqlServer=0, MySql=1, Sqlite=2. O cast (DatabaseType)appType
    // transformava Sqlite em MySql.
    public static DatabaseType ToInfrastructure(this AppDatabaseType appType) => appType switch
    {
        AppDatabaseType.SqlServer => DatabaseType.SqlServer,
        AppDatabaseType.MySql => DatabaseType.MySql,
        AppDatabaseType.Sqlite => DatabaseType.Sqlite,
        _ => throw new ArgumentOutOfRangeException(nameof(appType), appType, "Tipo de banco não suportado.")
    };

    public static AppDatabaseType ToApplication(this DatabaseType infrastructureType) => infrastructureType switch
    {
        DatabaseType.SqlServer => AppDatabaseType.SqlServer,
        DatabaseType.MySql => AppDatabaseType.MySql,
        DatabaseType.Sqlite => AppDatabaseType.Sqlite,
        _ => throw new ArgumentOutOfRangeException(nameof(infrastructureType), infrastructureType, "Tipo de banco não suportado.")
    };
}