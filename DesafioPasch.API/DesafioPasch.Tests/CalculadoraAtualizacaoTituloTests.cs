using DesafioPasch.Domain.Entities;
using DesafioPasch.Domain.Services;

namespace DesafioPasch.Tests;

public sealed class CalculadoraAtualizacaoTituloTests
{
    [Fact]
    public void Deve_calcular_multa_e_juros_por_parcela()
    {
        var hoje = new DateOnly(2025, 12, 11);

        // Parcela 1: 100 vencida há 10 dias => juros = 100*(0.01/30)*10 = 0.3333... => 0.33
        // Parcela 2: 200 vencida há 5 dias  => juros = 200*(0.01/30)*5  = 0.3333... => 0.33
        // juros total = 0.6666... => 0.67 (AwayFromZero)
        // valor original = 300.00
        // multa = 2% de 300 = 6.00
        // atualizado = 300 + 6 + 0.67 = 306.67

        var titulo = new Titulo(
            numero: "T1",
            nomeDevedor: "Devedor",
            parcelas: new[]
            {
                new Parcela(1, 100m, hoje.AddDays(-10), paga: false),
                new Parcela(2, 200m, hoje.AddDays(-5), paga: false)
            });

        var r = CalculadoraAtualizacaoTitulo.Calcular(titulo, hoje);

        Assert.Equal(300.00m, r.ValorOriginal);
        Assert.Equal(6.00m, r.Multa);
        Assert.Equal(0.67m, r.JurosTotal);
        Assert.Equal(306.67m, r.ValorAtualizado);
        Assert.Equal(10, r.DiasEmAtraso); // maior atraso entre parcelas
    }

    [Fact]
    public void Se_nao_estiver_em_atraso_deve_zerar_multa_e_juros()
    {
        var hoje = new DateOnly(2025, 12, 11);

        var titulo = new Titulo(
            numero: "T2",
            nomeDevedor: "Devedor",
            parcelas: new[]
            {
                new Parcela(1, 100m, hoje.AddDays(1), paga: false)
            });

        var r = CalculadoraAtualizacaoTitulo.Calcular(titulo, hoje);

        Assert.Equal(100.00m, r.ValorOriginal);
        Assert.Equal(0.00m, r.Multa);
        Assert.Equal(0.00m, r.JurosTotal);
        Assert.Equal(100.00m, r.ValorAtualizado);
        Assert.Equal(0, r.DiasEmAtraso);
    }

    [Fact]
    public void Deve_arredondar_away_from_zero_em_duas_casas()
    {
        var hoje = new DateOnly(2025, 12, 11);

        // Juros: 30*(0.01/30)*1 = 0.01
        var titulo = new Titulo(
            numero: "T3",
            nomeDevedor: "Devedor",
            parcelas: new[]
            {
            new Parcela(1, 30m, hoje.AddDays(-1), paga: false)
            });

        var r = CalculadoraAtualizacaoTitulo.Calcular(titulo, hoje);

        Assert.Equal(30.00m, r.ValorOriginal);
        Assert.Equal(0.60m, r.Multa);      // 2%
        Assert.Equal(0.01m, r.JurosTotal); // arredondamento correto
        Assert.Equal(30.61m, r.ValorAtualizado);
        Assert.Equal(1, r.DiasEmAtraso);
    }

    [Fact]
    public void Parcela_paga_nao_deve_contar_como_atraso()
    {
        var hoje = new DateOnly(2025, 12, 11);

        var titulo = new Titulo(
            numero: "T4",
            nomeDevedor: "Devedor",
            parcelas: new[]
            {
                new Parcela(1, 100m, hoje.AddDays(-30), paga: true) // vencida, mas paga
            });

        var r = CalculadoraAtualizacaoTitulo.Calcular(titulo, hoje);

        Assert.Equal(100.00m, r.ValorOriginal);
        Assert.Equal(0.00m, r.Multa);
        Assert.Equal(0.00m, r.JurosTotal);
        Assert.Equal(100.00m, r.ValorAtualizado);
        Assert.Equal(0, r.DiasEmAtraso);
    }

    [Fact]
    public void Titulo_com_parcela_paga_e_outra_em_atraso_deve_calcular_apenas_sobre_a_em_atraso()
    {
        var hoje = new DateOnly(2025, 12, 11);

        var titulo = new Titulo(
            numero: "T5",
            nomeDevedor: "Devedor",
            parcelas: new[]
            {
            new Parcela(1, 100m, hoje.AddDays(-10), paga: false), // conta
            new Parcela(2, 200m, hoje.AddDays(-10), paga: true)   // não conta
            });

        var r = CalculadoraAtualizacaoTitulo.Calcular(titulo, hoje);

        // valor original soma tudo
        Assert.Equal(300.00m, r.ValorOriginal);

        // multa 2% do total, pois título está em atraso (há parcela não paga vencida)
        Assert.Equal(6.00m, r.Multa);

        // juros só da parcela 1: 100*(0.01/30)*10 = 0.3333.. => 0.33
        Assert.Equal(0.33m, r.JurosTotal);

        Assert.Equal(306.33m, r.ValorAtualizado);
        Assert.Equal(10, r.DiasEmAtraso);
    }
}
