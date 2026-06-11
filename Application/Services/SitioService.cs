using Turismo.Application.Interfaces;
using Turismo.Domain.Entities;

namespace Turismo.Application.Services;

public class SitioService
{
    private readonly ISitioRepository _repository;

    public SitioService(ISitioRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Sitio>> GetAllAsync() => _repository.GetAllAsync();
    public Task<Sitio?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<IReadOnlyList<Sitio>> GetByUbicacionAsync(int ubicacionId) => _repository.GetByUbicacionAsync(ubicacionId);
    public Task<int> CreateAsync(Sitio sitio) => _repository.CreateAsync(sitio);
    public Task UpdateAsync(Sitio sitio) => _repository.UpdateAsync(sitio);
    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}
