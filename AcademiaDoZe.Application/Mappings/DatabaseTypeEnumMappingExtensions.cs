// Jonathan de Souza Pereira
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Application.Mappings;

public static class DatabaseTypeEnumMappingExtensions
{
    // Converte o enum da camada Application
    // para o enum da camada Infrastructure.
    public static DatabaseType ToInfrastructure(this AppDatabaseType appType)
    {
        return (DatabaseType)appType;
    }

    // Converte o enum da camada Infrastructure
    // para o enum da camada Application.
    public static AppDatabaseType ToApplication(this DatabaseType infrastructureType)
    {
        return (AppDatabaseType)infrastructureType;
    }
}