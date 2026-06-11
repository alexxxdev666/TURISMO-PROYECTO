using Turismo.Application.Interfaces;
using Turismo.Domain.Entities;

namespace Turismo.Application.Services;

public class CostoService
{
    private readonly ICostoRepository _repository;

    public CostoService(ICostoRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Costo>> GetAllAsync() => _repository.GetAllAsync();
    public Task<Costo?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<IReadOnlyList<Costo>> GetBySitioAsync(int sitioId) => _repository.GetBySitioAsync(sitioId);
    public Task<int> CreateAsync(Costo costo) => _repository.CreateAsync(costo);
    public Task UpdateAsync(Costo costo) => _repository.UpdateAsync(costo);
    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}
