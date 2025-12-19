namespace DesafioPasch.Domain.Entities;

public sealed class Parcela
{
    public int Numero { get; }
    public decimal Valor { get; }
    public DateOnly Vencimento { get; }
    public bool Paga { get; private set; }

    public Parcela(int numero, decimal valor, DateOnly vencimento, bool paga = false)
    {
        if (numero <= 0) throw new ArgumentOutOfRangeException(nameof(numero));
        if (valor <= 0) throw new ArgumentOutOfRangeException(nameof(valor));

        Numero = numero;
        Valor = valor;
        Vencimento = vencimento;
        Paga = paga;
    }

    public bool EstaEmAtraso(DateOnly hoje) => !Paga && Vencimento < hoje;

    public int DiasEmAtraso(DateOnly hoje) => EstaEmAtraso(hoje) ? hoje.DayNumber - Vencimento.DayNumber : 0;
}