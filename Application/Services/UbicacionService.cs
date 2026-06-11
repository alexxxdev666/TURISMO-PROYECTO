using Turismo.Application.Interfaces;
using Turismo.Domain.Entities;

namespace Turismo.Application.Services;

public class UbicacionService
{
    private readonly IUbicacionRepository _repository;

    public UbicacionService(IUbicacionRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Ubicacion>> GetAllAsync() => _repository.GetAllAsync();
    public Task<Ubicacion?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<int> CreateAsync(Ubicacion ubicacion) => _repository.CreateAsync(ubicacion);
    public Task UpdateAsync(Ubicacion ubicacion) => _repository.UpdateAsync(ubicacion);
    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}
