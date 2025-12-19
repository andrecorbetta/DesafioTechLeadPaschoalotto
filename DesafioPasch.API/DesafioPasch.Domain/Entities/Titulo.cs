namespace DesafioPasch.Domain.Entities;

public sealed class Titulo
{
    public string Numero { get; }
    public string NomeDevedor { get; }
    private readonly List<Parcela> _parcelas = new();
    public IReadOnlyList<Parcela> Parcelas => _parcelas;

    public Titulo(string numero, string nomeDevedor, IEnumerable<Parcela> parcelas)
    {
        Numero = string.IsNullOrWhiteSpace(numero) ? throw new ArgumentException("Número inválido.") : numero;
        NomeDevedor = string.IsNullOrWhiteSpace(nomeDevedor) ? throw new ArgumentException("Devedor inválido.") : nomeDevedor;

        _parcelas.AddRange(parcelas ?? throw new ArgumentNullException(nameof(parcelas)));
        if (_parcelas.Count == 0) throw new ArgumentException("Título deve possuir ao menos uma parcela.");
    }

    public bool EstaEmAtraso(DateOnly hoje) => _parcelas.Any(p => p.EstaEmAtraso(hoje));
}