using System.Data;

namespace Turismo.Application.Interfaces;

public interface IDbConnectionFactory
{
    string ProviderName { get; }
    bool IsSqlite { get; }
    IDbConnection CreateConnection();
}
