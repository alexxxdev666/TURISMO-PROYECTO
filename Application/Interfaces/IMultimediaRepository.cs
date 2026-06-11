using Turismo.Domain.Entities;

namespace Turismo.Application.Interfaces;

public interface IMultimediaRepository
{
    Task<IReadOnlyList<Multimedia>> GetBySitioAsync(int sitioId);
    Task<Multimedia?> GetByIdAsync(int id);
    Task<int> CreateAsync(Multimedia entity);
    Task DeleteAsync(int id);
}