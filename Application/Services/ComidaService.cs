using Turismo.Application.Interfaces;
using Turismo.Domain.Entities;

namespace Turismo.Application.Services;

public class ComidaService
{
    private readonly IComidaRepository _repository;

    public ComidaService(IComidaRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Comida>> GetAllAsync() => _repository.GetAllAsync();
    public Task<IReadOnlyList<Comida>> GetBySitioAsync(int sitioId) => _repository.GetBySitioAsync(sitioId);
    public Task<IReadOnlyList<SitioComida>> GetSitioComidasAsync(int sitioId) => _repository.GetSitioComidasAsync(sitioId);
    public Task<Comida?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<int> CreateAsync(Comida comida) => _repository.CreateAsync(comida);
    public Task ReplaceBySitioAsync(int sitioId, IReadOnlyList<SitioComida> comidaSitio) => _repository.ReplaceBySitioAsync(sitioId, comidaSitio);
    public Task UpdateAsync(Comida comida) => _repository.UpdateAsync(comida);
    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}
