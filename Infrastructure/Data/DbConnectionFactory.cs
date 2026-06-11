using System.Data;
using System.Data.OleDb;
using Microsoft.Data.Sqlite;
using Turismo.Application.Interfaces;

namespace Turismo.Infrastructure.Data;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly string _contentRootPath;

    public DbConnectionFactory(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _contentRootPath = environment.ContentRootPath;
    }

    public string ProviderName =>
        _configuration["DatabaseProvider"]
        ?? _configuration["DATABASE_PROVIDER"]
        ?? "Sqlite";

    public IDbConnection CreateConnection()
    {
        if (IsSqlite)
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultDb")
                ?? _configuration["SQLITE_CONNECTION_STRING"]
                ?? "Data Source=|DataDirectory|/turismo.sqlite";

            return new SqliteConnection(ResolveDataDirectory(connectionString));
        }

        var accessConnectionString =
            _configuration.GetConnectionString("AccessDb")
            ?? "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\\turismo.accdb;Persist Security Info=False;";

        return new OleDbConnection(ResolveDataDirectory(accessConnectionString));
    }

    public bool IsSqlite =>
        string.Equals(ProviderName, "Sqlite", StringComparison.OrdinalIgnoreCase);

    private string ResolveDataDirectory(string connectionString)
    {
        return connectionString.Replace(
            "|DataDirectory|",
            Path.Combine(_contentRootPath, "data"),
            StringComparison.OrdinalIgnoreCase);
    }
}
