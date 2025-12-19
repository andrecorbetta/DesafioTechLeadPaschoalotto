
using DesafioPasch.Application.Contracts;
using DesafioPasch.Application.Dtos;
using DesafioPasch.Domain.Services;

namespace DesafioPasch.Application.Services;

public sealed class TitulosEmAtrasoService
{
    private readonly ITituloRepository _repo;
    private readonly IClock _clock;

    public TitulosEmAtrasoService(ITituloRepository repo, IClock clock)
    {
        _repo = repo;
        _clock = clock;
    }

    public async Task<IReadOnlyList<TituloEmAtrasoDto>> ListarAsync(TitulosEmAtrasoQuery query, CancellationToken ct)
    {
        var hoje = query.DataBase ?? _clock.Hoje();
        var titulos = await _repo.ListarAsync(ct);

        IEnumerable<Domain.Entities.Titulo> baseTitulos = titulos;

        if (!string.IsNullOrWhiteSpace(query.NumeroTitulo))
        {
            var term = query.NumeroTitulo.Trim();
            baseTitulos = baseTitulos.Where(t => t.Numero.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.NomeDevedor))
        {
            var term = query.NomeDevedor.Trim();
            baseTitulos = baseTitulos.Where(t => t.NomeDevedor.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var baseList = baseTitulos
            .Where(t => t.EstaEmAtraso(hoje))
            .Select(t =>
            {
                var r = CalculadoraAtualizacaoTitulo.Calcular(t, hoje);
                return new TituloEmAtrasoDto(
                    NumeroTitulo: t.Numero,
                    NomeDevedor: t.NomeDevedor,
                    QuantidadeParcelas: t.Parcelas.Count,
                    ValorOriginal: r.ValorOriginal,
                    DiasEmAtraso: r.DiasEmAtraso,
                    ValorAtualizado: r.ValorAtualizado,
                    Multa: r.Multa,
                    JurosTotais: r.JurosTotal
                );
            });

        if (query.MinValorAtualizado is not null)
        {
            baseList = baseList.Where(x => x.ValorAtualizado >= query.MinValorAtualizado.Value);
        }

        if (query.MaxValorAtualizado is not null)
        {
            baseList = baseList.Where(x => x.ValorAtualizado <= query.MaxValorAtualizado.Value);
        }

        if (query.MinDiasAtraso is not null)
        {
            baseList = baseList.Where(x => x.DiasEmAtraso >= query.MinDiasAtraso.Value);
        }

        if (query.MaxDiasAtraso is not null)
        {
            baseList = baseList.Where(x => x.DiasEmAtraso <= query.MaxDiasAtraso.Value);
        }

        var sortBy = (query.SortBy ?? "diasAtraso").Trim().ToLowerInvariant();
        var desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        baseList = (sortBy, desc) switch
        {
            ("valoratualizado", false) => baseList.OrderBy(x => x.ValorAtualizado),
            ("valoratualizado", true) => baseList.OrderByDescending(x => x.ValorAtualizado),

            ("diasatraso", false) => baseList.OrderBy(x => x.DiasEmAtraso),
            ("diasatraso", true) => baseList.OrderByDescending(x => x.DiasEmAtraso),

            ("nomedevedor", false) => baseList.OrderBy(x => x.NomeDevedor),
            ("nomedevedor", true) => baseList.OrderByDescending(x => x.NomeDevedor),

            ("numerotitulo", false) => baseList.OrderBy(x => x.NumeroTitulo),
            ("numerotitulo", true) => baseList.OrderByDescending(x => x.NumeroTitulo),

            ("valororiginal", false) => baseList.OrderBy(x => x.ValorOriginal),
            ("valororiginal", true) => baseList.OrderByDescending(x => x.ValorOriginal),

            _ => desc ? baseList.OrderByDescending(x => x.DiasEmAtraso) : baseList.OrderBy(x => x.DiasEmAtraso)
        };

        return baseList.ToList();
    }
}