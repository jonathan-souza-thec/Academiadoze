// Jonathan de Souza Pereira

using AcademiaDoZe.Domain.Common;
namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity, IAggregateRoot
{
    public int ColaboradorId { get; private set; }
    public DateTime DataHora { get; private set; }          // entrada
    public DateTime? DataHoraSaida { get; private set; }

    public bool EstaDentro => DataHoraSaida is null;
    public TimeSpan? TempoPermanencia =>
        DataHoraSaida.HasValue ? DataHoraSaida.Value - DataHora : null;

    private AcessoColaborador(int id, int colaboradorId, DateTime dataHora, DateTime? dataHoraSaida) : base(id)
    {
        ColaboradorId = colaboradorId;
        DataHora = dataHora;
        DataHoraSaida = dataHoraSaida;
    }

    public static Result<AcessoColaborador> Criar(int id, Colaborador colaborador, DateTime dataHora)
    {
        var notifications = new List<Notification>();
        if (colaborador == null)
            notifications.Add(new Notification("Colaborador", "COLABORADOR_INVALIDO"));
        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) || dataHora.TimeOfDay > new TimeSpan(22, 0, 0))
            notifications.Add(new Notification("DataHora", "DATA_HORA_INTERVALO_INVALIDO"));
        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);
        return Result<AcessoColaborador>.Success(new AcessoColaborador(id, colaborador!.Id, dataHora, null));
    }

    public static AcessoColaborador Reconstituir(int id, int colaboradorId, DateTime entrada, DateTime? saida)
        => new(id, colaboradorId, entrada, saida);

    public Result<AcessoColaborador> RegistrarSaida(DateTime saida)
    {
        if (DataHoraSaida.HasValue)
            return Result<AcessoColaborador>.Failure("DataHoraSaida", "SAIDA_JA_REGISTRADA");
        if (saida < DataHora)
            return Result<AcessoColaborador>.Failure("DataHoraSaida", "SAIDA_ANTERIOR_A_ENTRADA");
        DataHoraSaida = saida;
        return Result<AcessoColaborador>.Success(this);
    }
}