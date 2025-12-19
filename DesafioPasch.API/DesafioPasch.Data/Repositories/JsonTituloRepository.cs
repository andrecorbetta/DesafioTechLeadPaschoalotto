using DesafioPasch.Application.Contracts;
using DesafioPasch.Data.JsonModels;
using DesafioPasch.Domain.Entities;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DesafioPasch.Data.Repositories;

public sealed class JsonTituloRepository : ITituloRepository
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly DataFilesOptions _options;

    public JsonTituloRepository(IOptions<DataFilesOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IReadOnlyList<Titulo>> ListarAsync(CancellationToken ct)
    {
        var path = ResolvePath(_options.TitulosPath);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Arquivo de títulos não encontrado: {path}");

        await using var stream = File.OpenRead(path);

        var titulosJson = await JsonSerializer.DeserializeAsync<List<TituloJson>>(stream, _jsonOptions, ct)
                        ?? new List<TituloJson>();

        var result = titulosJson.Select(t =>
            new Titulo(
                t.Numero,
                t.NomeDevedor,
                t.Parcelas.Select(p => new Parcela(p.Numero, p.Valor, p.Vencimento, p.Paga))
            )
        ).ToList();

        return result;
    }

    private static string ResolvePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.Combine(AppContext.BaseDirectory, configuredPath);
    }
}

public sealed class DataFilesOptions
{
    public string TitulosPath { get; set; } = "Data/titulos.json";
}