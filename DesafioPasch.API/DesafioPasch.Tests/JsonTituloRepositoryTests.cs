using DesafioPasch.Data.Repositories;
using Microsoft.Extensions.Options;

namespace DesafioPasch.Tests;

public sealed class JsonTituloRepositoryTests
{
    [Fact]
    public async Task Deve_ler_json_e_converter_para_domain()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"titulos-{Guid.NewGuid():N}.json");

        try
        {
            var payload = new[]
            {
                new
                {
                    numero = "TIT-9001",
                    nomeDevedor = "Teste",
                    parcelas = new[]
                    {
                        new { numero = 1, valor = 100.0m, vencimento = "2025-12-01", paga = false },
                        new { numero = 2, valor = 200.0m, vencimento = "2025-12-10", paga = true },
                    }
                }
            };

            await File.WriteAllTextAsync(tmp, System.Text.Json.JsonSerializer.Serialize(payload));

            var options = Options.Create(new DataFilesOptions { TitulosPath = tmp });
            var repo = new JsonTituloRepository(options);

            var result = await repo.ListarAsync(CancellationToken.None);

            Assert.Single(result);
            Assert.Equal("TIT-9001", result[0].Numero);
            Assert.Equal("Teste", result[0].NomeDevedor);
            Assert.Equal(2, result[0].Parcelas.Count);
            Assert.Equal(1, result[0].Parcelas[0].Numero);
            Assert.Equal(100m, result[0].Parcelas[0].Valor);
        }
        finally
        {
            if (File.Exists(tmp))
                File.Delete(tmp);
        }
    }

    [Fact]
    public async Task Deve_lancar_excecao_se_arquivo_nao_existir()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"naoexiste-{Guid.NewGuid():N}.json");

        var options = Options.Create(new DataFilesOptions { TitulosPath = tmp });
        var repo = new JsonTituloRepository(options);

        var ex = await Record.ExceptionAsync(() => repo.ListarAsync(CancellationToken.None));

        Assert.NotNull(ex);
        Assert.IsType<FileNotFoundException>(ex);
        Assert.Contains("Arquivo de títulos não encontrado", ex.Message);
    }
}
