namespace DesafioPasch.Data.JsonModels;

public sealed class TituloJson
{
    public string Numero { get; set; } = default!;
    public string NomeDevedor { get; set; } = default!;
    public List<ParcelaJson> Parcelas { get; set; } = new();
}