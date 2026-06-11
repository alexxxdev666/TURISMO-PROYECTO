using System.Data;
using System.Data.OleDb;
using Turismo.Application.Interfaces;
using Turismo.Domain.Entities;

namespace Turismo.Infrastructure.Repositories;

public class SitioRepository : ISitioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SitioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IReadOnlyList<Sitio>> GetAllAsync()
    {
        var results = new List<Sitio>();
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre, Descripcion, UbicacionId, Estado, FechaCreacion FROM Sitio";
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(Map(reader));
        }
        return Task.FromResult<IReadOnlyList<Sitio>>(results);
    }

    public Task<Sitio?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre, Descripcion, UbicacionId, Estado, FechaCreacion FROM Sitio WHERE Id = ?";
        command.Parameters.Add(new OleDbParameter { Value = id });
        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader != null && reader.Read())
        {
            return Task.FromResult<Sitio?>(Map(reader));
        }
        return Task.FromResult<Sitio?>(null);
    }

    public Task<IReadOnlyList<Sitio>> GetByUbicacionAsync(int ubicacionId)
    {
        var results = new List<Sitio>();
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre, Descripcion, UbicacionId, Estado, FechaCreacion FROM Sitio WHERE UbicacionId = ?";
        command.Parameters.Add(new OleDbParameter { Value = ubicacionId });
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(Map(reader));
        }
        return Task.FromResult<IReadOnlyList<Sitio>>(results);
    }

    public Task<int> CreateAsync(Sitio entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Sitio (Nombre, Descripcion, UbicacionId, Estado, FechaCreacion) VALUES (?, ?, ?, ?, ?)";
        command.Parameters.Add(new OleDbParameter { Value = entity.Nombre });
        command.Parameters.Add(new OleDbParameter { Value = (object?)entity.Descripcion ?? DBNull.Value });
        command.Parameters.Add(new OleDbParameter { Value = entity.UbicacionId });
        command.Parameters.Add(new OleDbParameter { Value = entity.Estado });
        command.Parameters.Add(new OleDbParameter { Value = entity.FechaCreacion });
        connection.Open();
        command.ExecuteNonQuery();
        command.CommandText = "SELECT @@IDENTITY";
        var result = command.ExecuteScalar();
        return Task.FromResult(Convert.ToInt32(result));
    }

    public Task UpdateAsync(Sitio entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Sitio SET Nombre = ?, Descripcion = ?, UbicacionId = ?, Estado = ?, FechaCreacion = ? WHERE Id = ?";
        command.Parameters.Add(new OleDbParameter { Value = entity.Nombre });
        command.Parameters.Add(new OleDbParameter { Value = (object?)entity.Descripcion ?? DBNull.Value });
        command.Parameters.Add(new OleDbParameter { Value = entity.UbicacionId });
        command.Parameters.Add(new OleDbParameter { Value = entity.Estado });
        command.Parameters.Add(new OleDbParameter { Value = entity.FechaCreacion });
        command.Parameters.Add(new OleDbParameter { Value = entity.Id });
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Sitio WHERE Id = ?";
        command.Parameters.Add(new OleDbParameter { Value = id });
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    private static Sitio Map(IDataRecord record)
    {
        return new Sitio
        {
            Id = record.GetInt32(0),
            Nombre = record.GetString(1),
            Descripcion = record.IsDBNull(2) ? null : record.GetString(2),
            UbicacionId = record.GetInt32(3),
            Estado = record.GetString(4),
            FechaCreacion = record.GetDateTime(5)
        };
    }
}
