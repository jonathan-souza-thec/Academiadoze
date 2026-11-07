using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity, IAggregateRoot
{
    public int AlunoId { get; private set; }

    // Data e hora da entrada
    public DateTime DataHora { get; private set; }

    // Data e hora da saída
    public DateTime? DataSaida { get; private set; }

    // Alias para compatibilidade com o DTO que usa Saida
    public DateTime? Saida => DataSaida;

    // Tempo total de permanência
    public TimeSpan? TempoPermanencia =>
        DataSaida.HasValue
            ? DataSaida.Value - DataHora
            : null;

    private AcessoAluno(
        int id,
        int alunoId,
        DateTime dataHora,
        DateTime? dataSaida = null) : base(id)
    {
        AlunoId = alunoId;
        DataHora = dataHora;
        DataSaida = dataSaida;
    }

    public static Result<AcessoAluno> Criar(
        int id,
        Aluno aluno,
        DateTime dataHora)
    {
        var notifications = new List<Notification>();

        if (aluno == null)
        {
            notifications.Add(
                new Notification("Aluno", "ALUNO_INVALIDO"));
        }

        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) ||
            dataHora.TimeOfDay > new TimeSpan(22, 0, 0))
        {
            notifications.Add(
                new Notification("DataHora", "DATA_HORA_INTERVALO_INVALIDO"));
        }

        if (notifications.Count != 0)
            return Result<AcessoAluno>.Failure(notifications);

        return Result<AcessoAluno>.Success(
            new AcessoAluno(id, aluno!.Id, dataHora));
    }

    // Reconstrói o acesso quando os dados vêm do banco
    public static AcessoAluno Reconstituir(
        int id,
        int alunoId,
        DateTime dataHora,
        DateTime? dataSaida)
    {
        return new AcessoAluno(
            id,
            alunoId,
            dataHora,
            dataSaida);
    }

    // Registra a saída do aluno
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