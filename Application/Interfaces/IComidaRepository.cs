using Turismo.Domain.Entities;

namespace Turismo.Application.Interfaces;

public interface IComidaRepository : ICrudRepository<Comida>
{
	Task<IReadOnlyList<Comida>> GetBySitioAsync(int sitioId);
	Task<IReadOnlyList<SitioComida>> GetSitioComidasAsync(int sitioId);
	Task ReplaceBySitioAsync(int sitioId, IReadOnlyList<SitioComida> comidaSitio);
}
