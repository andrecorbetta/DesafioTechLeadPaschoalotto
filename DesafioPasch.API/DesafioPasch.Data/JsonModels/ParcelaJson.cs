namespace DesafioPasch.Data.JsonModels;

public sealed class ParcelaJson
{
    public int Numero { get; set; }
    public decimal Valor { get; set; }
    public DateOnly Vencimento { get; set; }
    public bool Paga { get; set; }
}