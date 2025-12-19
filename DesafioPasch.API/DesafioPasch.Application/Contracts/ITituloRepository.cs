using DesafioPasch.Domain.Entities;

namespace DesafioPasch.Application.Contracts;

public interface ITituloRepository
{
    Task<IReadOnlyList<Titulo>> ListarAsync(CancellationToken ct);
}