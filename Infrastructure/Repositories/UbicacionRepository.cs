using System.Data;
using System.Data.OleDb;
using Turismo.Application.Interfaces;
using Turismo.Domain.Entities;

namespace Turismo.Infrastructure.Repositories;

public class UbicacionRepository : IUbicacionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UbicacionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IReadOnlyList<Ubicacion>> GetAllAsync()
    {
        var results = new List<Ubicacion>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Ubicacion ORDER BY Nombre";
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(MapRecord(reader));
        }

        return Task.FromResult<IReadOnlyList<Ubicacion>>(results);
    }

    public Task<Ubicacion?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Ubicacion WHERE Id = ?";
        command.Parameters.Add(new OleDbParameter { Value = id });
        using var reader = command.ExecuteReader();
        if (reader != null && reader.Read())
        {
            return Task.FromResult<Ubicacion?>(MapRecord(reader));
        }

        return Task.FromResult<Ubicacion?>(null);
    }

    public Task<int> CreateAsync(Ubicacion entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        object? result;
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Ubicacion (Nombre, Tipo, ParentId, Descripcion, Latitud, Longitud) VALUES (?, ?, ?, ?, ?, ?)";
            command.Parameters.Add(new OleDbParameter { Value = entity.Nombre });
            command.Parameters.Add(new OleDbParameter { Value = entity.Tipo });
            command.Parameters.Add(new OleDbParameter { Value = (object?)entity.ParentId ?? DBNull.Value });
            command.Parameters.Add(new OleDbParameter { Value = (object?)entity.Descripcion ?? DBNull.Value });
            command.Parameters.Add(new OleDbParameter { Value = entity.Latitud.HasValue ? entity.Latitud.Value : DBNull.Value });
            command.Parameters.Add(new OleDbParameter { Value = entity.Longitud.HasValue ? entity.Longitud.Value : DBNull.Value });
            command.ExecuteNonQuery();
            command.CommandText = "SELECT @@IDENTITY";
            result = command.ExecuteScalar();
        }
        catch (OleDbException)
        {
            using var fallbackCommand = connection.CreateCommand();
            fallbackCommand.CommandText = "INSERT INTO Ubicacion (Nombre, Tipo, ParentId, Descripcion) VALUES (?, ?, ?, ?)";
            fallbackCommand.Parameters.Add(new OleDbParameter { Value = entity.Nombre });
            fallbackCommand.Parameters.Add(new OleDbParameter { Value = entity.Tipo });
            fallbackCommand.Parameters.Add(new OleDbParameter { Value = (object?)entity.ParentId ?? DBNull.Value });
            fallbackCommand.Parameters.Add(new OleDbParameter { Value = (object?)entity.Descripcion ?? DBNull.Value });
            fallbackCommand.ExecuteNonQuery();
            fallbackCommand.CommandText = "SELECT @@IDENTITY";
            result = fallbackCommand.ExecuteScalar();
        }

        return Task.FromResult(Convert.ToInt32(result));
    }

    public Task UpdateAsync(Ubicacion entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE Ubicacion SET Nombre = ?, Tipo = ?, ParentId = ?, Descripcion = ?, Latitud = ?, Longitud = ? WHERE Id = ?";
            command.Parameters.Add(new OleDbParameter { Value = entity.Nombre });
            command.Parameters.Add(new OleDbParameter { Value = entity.Tipo });
            command.Parameters.Add(new OleDbParameter { Value = (object?)entity.ParentId ?? DBNull.Value });
            command.Parameters.Add(new OleDbParameter { Value = (object?)entity.Descripcion ?? DBNull.Value });
            command.Parameters.Add(new OleDbParameter { Value = entity.Latitud.HasValue ? entity.Latitud.Value : DBNull.Value });
            command.Parameters.Add(new OleDbParameter { Value = entity.Longitud.HasValue ? entity.Longitud.Value : DBNull.Value });
            command.Parameters.Add(new OleDbParameter { Value = entity.Id });
            command.ExecuteNonQuery();
        }
        catch (OleDbException)
        {
            using var fallbackCommand = connection.CreateCommand();
            fallbackCommand.CommandText = "UPDATE Ubicacion SET Nombre = ?, Tipo = ?, ParentId = ?, Descripcion = ? WHERE Id = ?";
            fallbackCommand.Parameters.Add(new OleDbParameter { Value = entity.Nombre });
            fallbackCommand.Parameters.Add(new OleDbParameter { Value = entity.Tipo });
            fallbackCommand.Parameters.Add(new OleDbParameter { Value = (object?)entity.ParentId ?? DBNull.Value });
            fallbackCommand.Parameters.Add(new OleDbParameter { Value = (object?)entity.Descripcion ?? DBNull.Value });
            fallbackCommand.Parameters.Add(new OleDbParameter { Value = entity.Id });
            fallbackCommand.ExecuteNonQuery();
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Ubicacion WHERE Id = ?";
        command.Parameters.Add(new OleDbParameter { Value = id });
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    private static Ubicacion MapRecord(IDataRecord record)
    {
        var latitudOrdinal = TryGetOrdinal(record, "Latitud");
        var longitudOrdinal = TryGetOrdinal(record, "Longitud");

        return new Ubicacion
        {
            Id = record.GetInt32(record.GetOrdinal("Id")),
            Nombre = record.GetString(record.GetOrdinal("Nombre")),
            Tipo = record.GetString(record.GetOrdinal("Tipo")),
            ParentId = ReadNullableInt(record, "ParentId"),
            Descripcion = ReadNullableString(record, "Descripcion"),
            Latitud = latitudOrdinal.HasValue && !record.IsDBNull(latitudOrdinal.Value)
                ? Convert.ToDouble(record.GetValue(latitudOrdinal.Value))
                : null,
            Longitud = longitudOrdinal.HasValue && !record.IsDBNull(longitudOrdinal.Value)
                ? Convert.ToDouble(record.GetValue(longitudOrdinal.Value))
                : null
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

    private static int? ReadNullableInt(IDataRecord record, string columnName)
    {
        var ordinal = TryGetOrdinal(record, columnName);
        return ordinal.HasValue && !record.IsDBNull(ordinal.Value)
            ? record.GetInt32(ordinal.Value)
            : null;
    }

    private static string? ReadNullableString(IDataRecord record, string columnName)
    {
        var ordinal = TryGetOrdinal(record, columnName);
        return ordinal.HasValue && !record.IsDBNull(ordinal.Value)
            ? record.GetString(ordinal.Value)
            : null;
    }
}
