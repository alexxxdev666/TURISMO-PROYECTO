using Turismo.Domain.Entities;

namespace Turismo.Application.Interfaces;

public interface ICostoRepository : ICrudRepository<Costo>
{
    Task<IReadOnlyList<Costo>> GetBySitioAsync(int sitioId);
}
