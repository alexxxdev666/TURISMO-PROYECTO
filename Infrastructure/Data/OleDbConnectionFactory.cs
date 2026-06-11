using System.Data;
using System.Data.OleDb;
using Microsoft.Extensions.Configuration;
using Turismo.Application.Interfaces;

namespace Turismo.Infrastructure.Data;

public class OleDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public OleDbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AccessDb") ?? string.Empty;
    }

    public IDbConnection CreateConnection()
    {
        return new OleDbConnection(_connectionString);
    }
}
