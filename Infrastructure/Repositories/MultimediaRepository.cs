using System.Data;
using Turismo.Application.Interfaces;
using Turismo.Infrastructure.Data;
using Turismo.Domain.Entities;

namespace Turismo.Infrastructure.Repositories;

public class MultimediaRepository : IMultimediaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MultimediaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IReadOnlyList<Multimedia>> GetBySitioAsync(int sitioId)
    {
        var results = new List<Multimedia>();
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, SitioId, Url, Tipo, Orden FROM Multimedia WHERE SitioId = ? ORDER BY Orden, Id";
        command.AddParameter(sitioId);
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(Map(reader));
        }

        return Task.FromResult<IReadOnlyList<Multimedia>>(results);
    }

    public Task<Multimedia?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, SitioId, Url, Tipo, Orden FROM Multimedia WHERE Id = ?";
        command.AddParameter(id);
        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader != null && reader.Read())
        {
            return Task.FromResult<Multimedia?>(Map(reader));
        }

        return Task.FromResult<Multimedia?>(null);
    }

    public Task<int> CreateAsync(Multimedia entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Multimedia (SitioId, Url, Tipo, Orden) VALUES (?, ?, ?, ?)";
        command.AddParameter(entity.SitioId);
        command.AddParameter(entity.Url);
        command.AddParameter(entity.Tipo);
        command.AddParameter(entity.Orden);
        connection.Open();
        command.ExecuteNonQuery();
        command.CommandText = connection.GetIdentityQuery();
        var result = command.ExecuteScalar();
        return Task.FromResult(Convert.ToInt32(result));
    }

    public Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Multimedia WHERE Id = ?";
        command.AddParameter(id);
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    private static Multimedia Map(IDataRecord record)
    {
        return new Multimedia
        {
            Id = record.GetInt32(0),
            SitioId = record.GetInt32(1),
            Url = record.GetString(2),
            Tipo = record.GetString(3),
            Orden = record.GetInt32(4)
        };
    }
}
