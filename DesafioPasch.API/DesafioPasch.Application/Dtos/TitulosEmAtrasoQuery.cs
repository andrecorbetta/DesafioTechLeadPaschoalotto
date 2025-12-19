namespace DesafioPasch.Application.Dtos;

/// <summary>
/// Parâmetros de consulta para filtragem e ordenação da listagem de títulos em atraso.
/// </summary>
public sealed class TitulosEmAtrasoQuery
{
    /// <summary>Filtra pelo número do título (contém, case-insensitive).</summary>
    public string? NumeroTitulo { get; init; }

    /// <summary>Filtra pelo nome do devedor (contém, case-insensitive).</summary>
    public string? NomeDevedor { get; init; }

    /// <summary>Filtra pelo valor atualizado mínimo.</summary>
    public decimal? MinValorAtualizado { get; init; }

    /// <summary>Filtra pelo valor atualizado máximo.</summary>
    public decimal? MaxValorAtualizado { get; init; }

    /// <summary>Filtra pela quantidade mínima de dias em atraso.</summary>
    public int? MinDiasAtraso { get; init; }

    /// <summary>Filtra pela quantidade máxima de dias em atraso.</summary>
    public int? MaxDiasAtraso { get; init; }

    /// <summary>
    /// Data base para o cálculo (simula "hoje"). Formato: yyyy-MM-dd.
    /// Útil para testes/homologação e validação dos cálculos.
    /// </summary>
    public DateOnly? DataBase { get; init; }

    /// <summary>
    /// Campo para ordenação:
    /// valorAtualizado | diasAtraso | nomeDevedor | numeroTitulo | valorOriginal
    /// </summary>
    public string? SortBy { get; init; }

    /// <summary>Direção da ordenação: asc | desc</summary>
    public string? SortDir { get; init; }
}