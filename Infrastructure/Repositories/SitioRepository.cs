using System.Data;
using Turismo.Application.Interfaces;
using Turismo.Infrastructure.Data;
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
        command.CommandText = "SELECT * FROM Sitio";
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
        command.CommandText = "SELECT * FROM Sitio WHERE Id = ?";
        command.AddParameter(id);
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
        command.CommandText = "SELECT * FROM Sitio WHERE UbicacionId = ?";
        command.AddParameter(ubicacionId);
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
        connection.Open();

        object? result;
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Sitio (Nombre, Descripcion, Actividades, UbicacionId, Estado, FechaCreacion) VALUES (?, ?, ?, ?, ?, ?)";
            command.AddParameter(entity.Nombre);
            command.AddParameter((object?)entity.Descripcion ?? DBNull.Value);
            command.AddParameter((object?)entity.Actividades ?? DBNull.Value);
            command.AddParameter(entity.UbicacionId);
            command.AddParameter(entity.Estado);
            command.AddParameter(entity.FechaCreacion);
            command.ExecuteNonQuery();
            command.CommandText = connection.GetIdentityQuery();
            result = command.ExecuteScalar();
        }
        catch (Exception) when (!connection.IsSqliteConnection())
        {
            using var fallbackCommand = connection.CreateCommand();
            fallbackCommand.CommandText = "INSERT INTO Sitio (Nombre, Descripcion, UbicacionId, Estado, FechaCreacion) VALUES (?, ?, ?, ?, ?)";
            fallbackCommand.AddParameter(entity.Nombre);
            fallbackCommand.AddParameter((object?)entity.Descripcion ?? DBNull.Value);
            fallbackCommand.AddParameter(entity.UbicacionId);
            fallbackCommand.AddParameter(entity.Estado);
            fallbackCommand.AddParameter(entity.FechaCreacion);
            fallbackCommand.ExecuteNonQuery();
            fallbackCommand.CommandText = connection.GetIdentityQuery();
            result = fallbackCommand.ExecuteScalar();
        }

        return Task.FromResult(Convert.ToInt32(result));
    }

    public Task UpdateAsync(Sitio entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Sitio SET Nombre = ?, Descripcion = ?, Actividades = ?, UbicacionId = ?, Estado = ?, FechaCreacion = ? WHERE Id = ?";
            command.AddParameter(entity.Nombre);
            command.AddParameter((object?)entity.Descripcion ?? DBNull.Value);
            command.AddParameter((object?)entity.Actividades ?? DBNull.Value);
            command.AddParameter(entity.UbicacionId);
            command.AddParameter(entity.Estado);
            command.AddParameter(entity.FechaCreacion);
            command.AddParameter(entity.Id);
            command.ExecuteNonQuery();
        }
        catch (Exception) when (!connection.IsSqliteConnection())
        {
            using var fallbackCommand = connection.CreateCommand();
            fallbackCommand.CommandText = "UPDATE Sitio SET Nombre = ?, Descripcion = ?, UbicacionId = ?, Estado = ?, FechaCreacion = ? WHERE Id = ?";
            fallbackCommand.AddParameter(entity.Nombre);
            fallbackCommand.AddParameter((object?)entity.Descripcion ?? DBNull.Value);
            fallbackCommand.AddParameter(entity.UbicacionId);
            fallbackCommand.AddParameter(entity.Estado);
            fallbackCommand.AddParameter(entity.FechaCreacion);
            fallbackCommand.AddParameter(entity.Id);
            fallbackCommand.ExecuteNonQuery();
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Sitio WHERE Id = ?";
        command.AddParameter(id);
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    private static Sitio Map(IDataRecord record)
    {
        var actividadesOrdinal = TryGetOrdinal(record, "Actividades");

        return new Sitio
        {
            Id = record.GetInt32(record.GetOrdinal("Id")),
            Nombre = record.GetString(record.GetOrdinal("Nombre")),
            Descripcion = ReadNullableString(record, "Descripcion"),
            Actividades = actividadesOrdinal.HasValue && !record.IsDBNull(actividadesOrdinal.Value)
                ? Convert.ToString(record.GetValue(actividadesOrdinal.Value))
                : null,
            UbicacionId = record.GetInt32(record.GetOrdinal("UbicacionId")),
            Estado = record.GetString(record.GetOrdinal("Estado")),
            FechaCreacion = record.GetDateTime(record.GetOrdinal("FechaCreacion"))
        };
    }

    private static int? TryGetOrdinal(IDataRecord record, string columnName)
    {
        try
        {
            return record.GetOrdinal(columnName);
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }

    private static string? ReadNullableString(IDataRecord record, string columnName)
    {
        var ordinal = TryGetOrdinal(record, columnName);
        return ordinal.HasValue && !record.IsDBNull(ordinal.Value)
            ? Convert.ToString(record.GetValue(ordinal.Value))
            : null;
    }
}

