using System.Data;
using Turismo.Application.Interfaces;
using Turismo.Infrastructure.Data;
using Turismo.Domain.Entities;

namespace Turismo.Infrastructure.Repositories;

public class ComidaRepository : IComidaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ComidaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IReadOnlyList<Comida>> GetAllAsync()
    {
        var results = new List<Comida>();
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre FROM Comida";
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(Map(reader));
        }
        return Task.FromResult<IReadOnlyList<Comida>>(results);
    }

    public Task<Comida?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre FROM Comida WHERE Id = ?";
        command.AddParameter(id);
        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader != null && reader.Read())
        {
            return Task.FromResult<Comida?>(Map(reader));
        }
        return Task.FromResult<Comida?>(null);
    }

    public Task<IReadOnlyList<Comida>> GetBySitioAsync(int sitioId)
    {
        var results = new List<Comida>();
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT c.Id, c.Nombre
                               FROM Comida c
                               INNER JOIN SitioComida sc ON c.Id = sc.ComidaId
                               WHERE sc.SitioId = ?
                               ORDER BY c.Nombre";
        command.AddParameter(sitioId);
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(Map(reader));
        }

        return Task.FromResult<IReadOnlyList<Comida>>(results);
    }

    public Task<IReadOnlyList<SitioComida>> GetSitioComidasAsync(int sitioId)
    {
        var results = new List<SitioComida>();
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT SitioId, ComidaId, ValorReferencial, Observacion
                               FROM SitioComida
                               WHERE SitioId = ?
                               ORDER BY ComidaId";
        command.AddParameter(sitioId);
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(new SitioComida
            {
                SitioId = reader.GetInt32(0),
                ComidaId = reader.GetInt32(1),
                ValorReferencial = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                Observacion = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }

        return Task.FromResult<IReadOnlyList<SitioComida>>(results);
    }

    public Task<int> CreateAsync(Comida entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Comida (Nombre) VALUES (?)";
        command.AddParameter(entity.Nombre);
        connection.Open();
        command.ExecuteNonQuery();
        command.CommandText = connection.GetIdentityQuery();
        var result = command.ExecuteScalar();
        return Task.FromResult(Convert.ToInt32(result));
    }

    public Task ReplaceBySitioAsync(int sitioId, IReadOnlyList<SitioComida> comidaSitio)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using (var deleteCommand = connection.CreateCommand())
        {
            deleteCommand.CommandText = "DELETE FROM SitioComida WHERE SitioId = ?";
            deleteCommand.AddParameter(sitioId);
            deleteCommand.ExecuteNonQuery();
        }

        var itemsUnicos = comidaSitio?
            .Where(item => item != null)
            .GroupBy(item => item.ComidaId)
            .Select(group => group.First())
            .ToList() ?? new List<SitioComida>();

        foreach (var item in itemsUnicos)
        {
            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO SitioComida (SitioId, ComidaId, ValorReferencial, Observacion) VALUES (?, ?, ?, ?)";
            insertCommand.AddParameter(sitioId);
            insertCommand.AddParameter(item.ComidaId);
            insertCommand.AddParameter(item.ValorReferencial.HasValue ? item.ValorReferencial.Value : DBNull.Value);
            insertCommand.AddParameter((object?)item.Observacion ?? DBNull.Value);
            insertCommand.ExecuteNonQuery();
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Comida entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Comida SET Nombre = ? WHERE Id = ?";
        command.AddParameter(entity.Nombre);
        command.AddParameter(entity.Id);
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Comida WHERE Id = ?";
        command.AddParameter(id);
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    private static Comida Map(IDataRecord record)
    {
        return new Comida
        {
            Id = record.GetInt32(0),
            Nombre = record.GetString(1)
        };
    }
}

