using System.Data;

namespace Turismo.Application.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
