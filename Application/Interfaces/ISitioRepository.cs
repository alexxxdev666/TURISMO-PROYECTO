using Turismo.Domain.Entities;

namespace Turismo.Application.Interfaces;

public interface ISitioRepository : ICrudRepository<Sitio>
{
    Task<IReadOnlyList<Sitio>> GetByUbicacionAsync(int ubicacionId);
}
