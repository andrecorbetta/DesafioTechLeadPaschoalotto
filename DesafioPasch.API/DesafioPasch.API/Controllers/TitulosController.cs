using DesafioPasch.Application.Dtos;
using DesafioPasch.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace DesafioPasch.API.Controllers;

/// <summary>
/// Operações de consulta de títulos.
/// </summary>
/// <remarks>
/// Esta controller expõe endpoints para consulta de títulos e cálculos relacionados.
/// A regra de cálculo (multa/juros/atualização) está isolada na camada de Domínio.
/// </remarks>
[ApiController]
[Route("v1/titulos")]
[Produces("application/json")]
public sealed class TitulosController : ControllerBase
{
    /// <summary>
    /// Lista títulos em atraso e calcula valores atualizados conforme regra do desafio.
    /// </summary>
    /// <remarks>
    /// Regras aplicadas:
    /// - Um título é considerado "em atraso" quando existir ao menos uma parcela vencida e não paga.
    /// - Valor original = soma do valor de todas as parcelas do título.
    /// - Multa = 2% sobre o valor original (aplicada apenas se houver atraso).
    /// - Juros = somatório, por parcela vencida e não paga, de: valorParcela * (0,01/30) * diasEmAtrasoDaParcela.
    /// - Valor atualizado = valor original + multa + juros.
    /// - Arredondamento: todos os valores monetários retornados são arredondados para 2 casas decimais usando AwayFromZero.
    /// - Dias em atraso (retornado) = maior atraso entre as parcelas vencidas e não pagas do título.
    ///
    /// Filtros e ordenação são opcionais e aplicados sobre a listagem retornada.
    ///
    /// Exemplos:
    /// - Filtrar por nome e ordenar por valor atualizado desc:
    ///   GET /v1/titulos/atrasados?nomeDevedor=joao&amp;sortBy=valorAtualizado&amp;sortDir=desc
    ///
    /// - Filtrar por intervalo de atraso:
    ///   GET /v1/titulos/atrasados?minDiasAtraso=10&amp;maxDiasAtraso=60
    ///
    /// - Simular data atual (útil para testes e homologação):
    ///   GET /v1/titulos/atrasados?dataBase=2025-12-11
    /// </remarks>
    /// <param name="query">Parâmetros opcionais de filtro, ordenação e data base.</param>
    /// <param name="service">Serviço de aplicação responsável por aplicar regra de listagem e cálculo.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Lista de títulos em atraso com valores calculados.</returns>
    /// <response code="200">Retorna a lista de títulos em atraso, já com cálculo de multa, juros e valor atualizado.</response>
    /// <response code="400">
    /// Retorna erro de validação quando:
    /// - minDiasAtraso &gt; maxDiasAtraso
    /// - minValorAtualizado &gt; maxValorAtualizado
    /// - sortDir diferente de 'asc' ou 'desc'
    /// </response>
    [HttpGet("atrasados")]
    [ProducesResponseType(typeof(IReadOnlyList<TituloEmAtrasoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAtrasados(
        [FromQuery] TitulosEmAtrasoQuery query,
        [FromServices] TitulosEmAtrasoService service,
        CancellationToken ct)
    {
        // validações mínimas (pro)
        if (query.MinDiasAtraso is not null && query.MaxDiasAtraso is not null && query.MinDiasAtraso > query.MaxDiasAtraso)
        {
            return BadRequest("minDiasAtraso não pode ser maior que maxDiasAtraso.");
        }

        if (query.MinValorAtualizado is not null && query.MaxValorAtualizado is not null && query.MinValorAtualizado > query.MaxValorAtualizado)
        {
            return BadRequest("minValorAtualizado não pode ser maior que maxValorAtualizado.");
        }

        if (!string.IsNullOrWhiteSpace(query.SortDir) &&
            !query.SortDir.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
            !query.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("sortDir deve ser 'asc' ou 'desc'.");
        }

        var result = await service.ListarAsync(query, ct);
        return Ok(result);
    }
}