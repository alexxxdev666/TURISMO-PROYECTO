using Turismo.Application.Interfaces;
using Turismo.Domain.Entities;

namespace Turismo.Application.Services;

public class UsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Usuario>> GetAllAsync() => _repository.GetAllAsync();
    public Task<Usuario?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<Usuario?> GetByEmailAsync(string email) => _repository.GetByEmailAsync(email);
    public Task<int> CreateAsync(Usuario usuario) => _repository.CreateAsync(usuario);
    public Task UpdateAsync(Usuario usuario) => _repository.UpdateAsync(usuario);
    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}
