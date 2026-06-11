using System.Data;
using System.Data.OleDb;
using Turismo.Application.Interfaces;
using Turismo.Domain.Entities;

namespace Turismo.Infrastructure.Repositories;

public class CostoRepository : ICostoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CostoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IReadOnlyList<Costo>> GetAllAsync()
    {
        var results = new List<Costo>();
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, SitioId, Tipo, Valor, Moneda, Observacion FROM Costo";
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(Map(reader));
        }
        return Task.FromResult<IReadOnlyList<Costo>>(results);
    }

    public Task<Costo?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, SitioId, Tipo, Valor, Moneda, Observacion FROM Costo WHERE Id = ?";
        command.Parameters.Add(new OleDbParameter { Value = id });
        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader != null && reader.Read())
        {
            return Task.FromResult<Costo?>(Map(reader));
        }
        return Task.FromResult<Costo?>(null);
    }

    public Task<IReadOnlyList<Costo>> GetBySitioAsync(int sitioId)
    {
        var results = new List<Costo>();
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, SitioId, Tipo, Valor, Moneda, Observacion FROM Costo WHERE SitioId = ?";
        command.Parameters.Add(new OleDbParameter { Value = sitioId });
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(Map(reader));
        }
        return Task.FromResult<IReadOnlyList<Costo>>(results);
    }

    public Task<int> CreateAsync(Costo entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Costo (SitioId, Tipo, Valor, Moneda, Observacion) VALUES (?, ?, ?, ?, ?)";
        command.Parameters.Add(new OleDbParameter { Value = entity.SitioId });
        command.Parameters.Add(new OleDbParameter { Value = entity.Tipo });
        command.Parameters.Add(new OleDbParameter { Value = entity.Valor });
        command.Parameters.Add(new OleDbParameter { Value = entity.Moneda });
        command.Parameters.Add(new OleDbParameter { Value = (object?)entity.Observacion ?? DBNull.Value });
        connection.Open();
        command.ExecuteNonQuery();
        command.CommandText = "SELECT @@IDENTITY";
        var result = command.ExecuteScalar();
        return Task.FromResult(Convert.ToInt32(result));
    }

    public Task UpdateAsync(Costo entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Costo SET SitioId = ?, Tipo = ?, Valor = ?, Moneda = ?, Observacion = ? WHERE Id = ?";
        command.Parameters.Add(new OleDbParameter { Value = entity.SitioId });
        command.Parameters.Add(new OleDbParameter { Value = entity.Tipo });
        command.Parameters.Add(new OleDbParameter { Value = entity.Valor });
        command.Parameters.Add(new OleDbParameter { Value = entity.Moneda });
        command.Parameters.Add(new OleDbParameter { Value = (object?)entity.Observacion ?? DBNull.Value });
        command.Parameters.Add(new OleDbParameter { Value = entity.Id });
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Costo WHERE Id = ?";
        command.Parameters.Add(new OleDbParameter { Value = id });
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    private static Costo Map(IDataRecord record)
    {
        return new Costo
        {
            Id = record.GetInt32(0),
            SitioId = record.GetInt32(1),
            Tipo = record.GetString(2),
            Valor = record.GetDecimal(3),
            Moneda = record.GetString(4),
            Observacion = record.IsDBNull(5) ? null : record.GetString(5)
        };
    }
}
