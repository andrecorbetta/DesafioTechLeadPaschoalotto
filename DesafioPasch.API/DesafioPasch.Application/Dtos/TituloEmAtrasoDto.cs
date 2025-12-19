namespace DesafioPasch.Application.Dtos;

/// <summary>
/// Representa um título em atraso com valores calculados conforme regras do desafio.
/// </summary>
/// <param name="NumeroTitulo">Identificador do título.</param>
/// <param name="NomeDevedor">Nome do devedor.</param>
/// <param name="QuantidadeParcelas">Quantidade total de parcelas do título.</param>
/// <param name="ValorOriginal">Soma do valor de todas as parcelas.</param>
/// <param name="DiasEmAtraso">Maior atraso (em dias) entre as parcelas vencidas e não pagas.</param>
/// <param name="ValorAtualizado">ValorOriginal + Multa + JurosTotais.</param>
/// <param name="Multa">2% sobre o ValorOriginal (apenas se houver atraso).</param>
/// <param name="JurosTotais">Somatório dos juros proporcionais por parcela vencida e não paga.</param>
public sealed record TituloEmAtrasoDto(
    string NumeroTitulo,
    string NomeDevedor,
    int QuantidadeParcelas,
    decimal ValorOriginal,
    int DiasEmAtraso,
    decimal ValorAtualizado,
    decimal Multa,
    decimal JurosTotais
);