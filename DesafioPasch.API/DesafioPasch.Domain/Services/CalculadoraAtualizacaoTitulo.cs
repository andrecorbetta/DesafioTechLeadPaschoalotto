using DesafioPasch.Domain.Entities;

namespace DesafioPasch.Domain.Services;

public static class CalculadoraAtualizacaoTitulo
{
    private const decimal MultaPercentual = 0.02m; // 2%
    private const decimal JurosAoMes = 0.01m;      // 1% ao mês
    private const decimal BaseDias = 30m;

    public static ResultadoAtualizacao Calcular(Titulo titulo, DateOnly hoje)
    {
        var valorOriginal = titulo.Parcelas.Sum(p => p.Valor);

        // Dias em atraso (para exibição): maior atraso entre parcelas vencidas (referência “data atual”)
        var diasEmAtraso = titulo.Parcelas.Max(p => p.DiasEmAtraso(hoje));

        var emAtraso = titulo.EstaEmAtraso(hoje);

        var multa = emAtraso ? valorOriginal * MultaPercentual : 0m;

        // Juros: (1%/30) proporcional por dia POR PARCELA
        var jurosTotal = 0m;
        foreach (var parcela in titulo.Parcelas)
        {
            var dias = parcela.DiasEmAtraso(hoje);
            if (dias <= 0) continue;

            jurosTotal += parcela.Valor * (JurosAoMes / BaseDias) * dias;
        }

        var valorAtualizado = valorOriginal + multa + jurosTotal;

        return new ResultadoAtualizacao(
            ValorOriginal: Arred2(valorOriginal),
            Multa: Arred2(multa),
            JurosTotal: Arred2(jurosTotal),
            ValorAtualizado: Arred2(valorAtualizado),
            DiasEmAtraso: diasEmAtraso
        );
    }

    private static decimal Arred2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}

public sealed record ResultadoAtualizacao(
    decimal ValorOriginal,
    decimal Multa,
    decimal JurosTotal,
    decimal ValorAtualizado,
    int DiasEmAtraso
);