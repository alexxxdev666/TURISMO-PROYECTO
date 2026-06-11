using Turismo.Domain.Entities;

namespace Turismo.Application.Interfaces;

public interface IUsuarioRepository : ICrudRepository<Usuario>
{
    Task<Usuario?> GetByEmailAsync(string email);
}
