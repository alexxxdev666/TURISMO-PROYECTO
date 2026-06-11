using System.Data;
using Turismo.Application.Interfaces;
using Turismo.Infrastructure.Data;
using Turismo.Domain.Entities;

namespace Turismo.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UsuarioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IReadOnlyList<Usuario>> GetAllAsync()
    {
        var results = new List<Usuario>();
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre, Email, PasswordHash, Activo FROM Usuarios";
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader != null && reader.Read())
        {
            results.Add(Map(reader));
        }
        return Task.FromResult<IReadOnlyList<Usuario>>(results);
    }

    public Task<Usuario?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre, Email, PasswordHash, Activo FROM Usuarios WHERE Id = ?";
        command.AddParameter(id);
        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader != null && reader.Read())
        {
            return Task.FromResult<Usuario?>(Map(reader));
        }
        return Task.FromResult<Usuario?>(null);
    }

    public Task<Usuario?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Nombre, Email, PasswordHash, Activo FROM Usuarios WHERE Email = ?";
        command.AddParameter(email);
        connection.Open();
        using var reader = command.ExecuteReader();
        if (reader != null && reader.Read())
        {
            return Task.FromResult<Usuario?>(Map(reader));
        }
        return Task.FromResult<Usuario?>(null);
    }

    public Task<int> CreateAsync(Usuario entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Usuarios (Nombre, Email, PasswordHash, Activo) VALUES (?, ?, ?, ?)";
        command.AddParameter(entity.Nombre);
        command.AddParameter(entity.Email);
        command.AddParameter(entity.PasswordHash);
        command.AddParameter(entity.Activo);
        connection.Open();
        command.ExecuteNonQuery();
        command.CommandText = connection.GetIdentityQuery();
        var result = command.ExecuteScalar();
        return Task.FromResult(Convert.ToInt32(result));
    }

    public Task UpdateAsync(Usuario entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Usuarios SET Nombre = ?, Email = ?, PasswordHash = ?, Activo = ? WHERE Id = ?";
        command.AddParameter(entity.Nombre);
        command.AddParameter(entity.Email);
        command.AddParameter(entity.PasswordHash);
        command.AddParameter(entity.Activo);
        command.AddParameter(entity.Id);
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Usuarios WHERE Id = ?";
        command.AddParameter(id);
        connection.Open();
        command.ExecuteNonQuery();
        return Task.CompletedTask;
    }

    private static Usuario Map(IDataRecord record)
    {
        return new Usuario
        {
            Id = record.GetInt32(0),
            Nombre = record.GetString(1),
            Email = record.GetString(2),
            PasswordHash = record.GetString(3),
            Activo = record.GetBoolean(4)
        };
    }
}

