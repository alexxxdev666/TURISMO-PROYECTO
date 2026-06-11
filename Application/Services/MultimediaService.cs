using Turismo.Application.Interfaces;
using Turismo.Domain.Entities;

namespace Turismo.Application.Services;

public class MultimediaService
{
    private readonly IMultimediaRepository _repository;

    public MultimediaService(IMultimediaRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Multimedia>> GetBySitioAsync(int sitioId) => _repository.GetBySitioAsync(sitioId);
    public Task<Multimedia?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<int> CreateAsync(Multimedia multimedia) => _repository.CreateAsync(multimedia);
    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}