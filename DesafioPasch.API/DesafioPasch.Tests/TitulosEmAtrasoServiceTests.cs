using DesafioPasch.Application.Contracts;
using DesafioPasch.Application.Dtos;
using DesafioPasch.Application.Services;
using DesafioPasch.Domain.Entities;

namespace DesafioPasch.Tests;

public sealed class TitulosEmAtrasoServiceTests
{
    [Fact]
    public async Task Deve_filtrar_por_nome_e_ordenar_por_valor_atualizado_desc()
    {
        var hoje = new DateOnly(2025, 12, 11);

        // T1: atrasado (100 venc -10d)
        var t1 = new Titulo("TIT-0001", "João Silva", new[]
        {
            new Parcela(1, 100m, hoje.AddDays(-10), paga: false)
        });

        // T2: atrasado (200 venc -5d)
        var t2 = new Titulo("TIT-0002", "João Pedro", new[]
        {
            new Parcela(1, 200m, hoje.AddDays(-5), paga: false)
        });

        // T3: não atrasado
        var t3 = new Titulo("TIT-0003", "Maria", new[]
        {
            new Parcela(1, 999m, hoje.AddDays(1), paga: false)
        });

        var repo = new FakeRepo(t1, t2, t3);
        var clock = new FakeClock(hoje);

        var svc = new TitulosEmAtrasoService(repo, clock);

        var query = new TitulosEmAtrasoQuery
        {
            NomeDevedor = "joão",
            SortBy = "valorAtualizado",
            SortDir = "desc"
        };

        var result = await svc.ListarAsync(query, CancellationToken.None);

        // Deve retornar apenas t1 e t2 (t3 não entra), e em ordem desc por valor atualizado
        Assert.Equal(2, result.Count);
        Assert.Equal("TIT-0002", result[0].NumeroTitulo);
        Assert.Equal("TIT-0001", result[1].NumeroTitulo);
    }

    [Fact]
    public async Task Deve_aplicar_filtros_de_intervalo()
    {
        var hoje = new DateOnly(2025, 12, 11);

        var t1 = new Titulo("TIT-0001", "A", new[] { new Parcela(1, 100m, hoje.AddDays(-1), paga: false) });  // 1 dia
        var t2 = new Titulo("TIT-0002", "B", new[] { new Parcela(1, 100m, hoje.AddDays(-40), paga: false) }); // 40 dias
        var t3 = new Titulo("TIT-0003", "C", new[] { new Parcela(1, 100m, hoje.AddDays(-80), paga: false) }); // 80 dias

        var svc = new TitulosEmAtrasoService(new FakeRepo(t1, t2, t3), new FakeClock(hoje));

        var query = new TitulosEmAtrasoQuery
        {
            MinDiasAtraso = 30,
            MaxDiasAtraso = 70,
            SortBy = "diasAtraso",
            SortDir = "asc"
        };

        var result = await svc.ListarAsync(query, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("TIT-0002", result[0].NumeroTitulo);
        Assert.InRange(result[0].DiasEmAtraso, 30, 70);
    }

    [Fact]
    public async Task Deve_respeitar_dataBase_para_calcular_atraso()
    {
        var baseDate = new DateOnly(2025, 12, 11);

        var titulo = new Titulo("TIT-0001", "A", new[]
        {
        new Parcela(1, 100m, new DateOnly(2025, 12, 10), paga: false)
    });

        var svc = new TitulosEmAtrasoService(new FakeRepo(titulo), new FakeClock(baseDate));

        var resultHoje = await svc.ListarAsync(new TitulosEmAtrasoQuery { DataBase = new DateOnly(2025, 12, 11) }, CancellationToken.None);
        Assert.Single(resultHoje);
        Assert.Equal(1, resultHoje[0].DiasEmAtraso);

        var resultOntem = await svc.ListarAsync(new TitulosEmAtrasoQuery { DataBase = new DateOnly(2025, 12, 10) }, CancellationToken.None);
        Assert.Empty(resultOntem); // no mesmo dia do vencimento, não é atraso
    }

    private sealed class FakeRepo : ITituloRepository
    {
        private readonly IReadOnlyList<Titulo> _data;
        public FakeRepo(params Titulo[] titulos) => _data = titulos;

        public Task<IReadOnlyList<Titulo>> ListarAsync(CancellationToken ct)
            => Task.FromResult(_data);
    }

    private sealed class FakeClock : IClock
    {
        private readonly DateOnly _hoje;
        public FakeClock(DateOnly hoje) => _hoje = hoje;
        public DateOnly Hoje() => _hoje;
    }
}
