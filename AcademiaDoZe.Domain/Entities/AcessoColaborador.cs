using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity, IAggregateRoot
{
    public int ColaboradorId { get; private set; }

    // Data e hora da entrada
    public DateTime DataHora { get; private set; }

    // Data e hora da saída
    public DateTime? DataSaida { get; private set; }

    // Alias para compatibilidade com o DTO
    public DateTime? Saida => DataSaida;

    // Tempo total de permanência
    public TimeSpan? TempoPermanencia =>
        DataSaida.HasValue
            ? DataSaida.Value - DataHora
            : null;

    private AcessoColaborador(
        int id,
        int colaboradorId,
        DateTime dataHora,
        DateTime? dataSaida = null) : base(id)
    {
        ColaboradorId = colaboradorId;
        DataHora = dataHora;
        DataSaida = dataSaida;
    }

    public static Result<AcessoColaborador> Criar(
        int id,
        Colaborador colaborador,
        DateTime dataHora)
    {
        var notifications = new List<Notification>();

        if (colaborador == null)
        {
            notifications.Add(
                new Notification("Colaborador", "COLABORADOR_INVALIDO"));
        }

        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) ||
            dataHora.TimeOfDay > new TimeSpan(22, 0, 0))
        {
            notifications.Add(
                new Notification("DataHora", "DATA_HORA_INTERVALO_INVALIDO"));
        }

        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);

        return Result<AcessoColaborador>.Success(
            new AcessoColaborador(
                id,
                colaborador!.Id,
                dataHora));
    }

    // Reconstrói o acesso quando os dados vêm do banco
    public static AcessoColaborador Reconstituir(
        int id,
        int colaboradorId,
        DateTime dataHora,
        DateTime? dataSaida)
    {
        return new AcessoColaborador(
            id,
            colaboradorId,
            dataHora,
            dataSaida);
    }

    // Registra a saída do colaborador
    public void RegistrarSaida(DateTime dataSaida)
    {
        if (DataSaida.HasValue)
            throw new InvalidOperationException(
                "A saída deste acesso já foi registrada.");

        if (dataSaida < DataHora)
            throw new InvalidOperationException(
                "A saída não pode ser anterior à entrada.");

        DataSaida = dataSaida;
    }
}