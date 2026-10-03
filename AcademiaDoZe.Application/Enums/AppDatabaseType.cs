// Jonathan de Souza Pereira
using System.ComponentModel.DataAnnotations;

namespace AcademiaDoZe.Application.Enums;

public enum AppDatabaseType
{
    [Display(Name = "SQL Server")]
    SqlServer = 0,
    [Display(Name = "SQLite")]
    Sqlite = 1,
    [Display(Name = "MySQL")]
    MySql = 2
}