// Jonathan de Souza Pereira

using AcademiaDoZe.Domain.Common;
namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity, IAggregateRoot
{
    public int AlunoId { get; private set; }
    // Data/hora da entrada (mantém o nome DataHora para não quebrar os testes existentes)
    public DateTime DataHora { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

    public bool EstaDentro => DataHoraSaida is null;
    public TimeSpan? TempoPermanencia =>
        DataHoraSaida.HasValue ? DataHoraSaida.Value - DataHora : null;

    private AcessoAluno(int id, int alunoId, DateTime dataHora, DateTime? dataHoraSaida) : base(id)
    {
        AlunoId = alunoId;
        DataHora = dataHora;
        DataHoraSaida = dataHoraSaida;
    }

    public static Result<AcessoAluno> Criar(int id, Aluno aluno, DateTime dataHora)
    {
        var notifications = new List<Notification>();
        if (aluno == null)
            notifications.Add(new Notification("Aluno", "ALUNO_INVALIDO"));
        if (dataHora.TimeOfDay < new TimeSpan(6, 0, 0) || dataHora.TimeOfDay > new TimeSpan(22, 0, 0))
            notifications.Add(new Notification("DataHora", "DATA_HORA_INTERVALO_INVALIDO"));
        if (notifications.Count != 0)
            return Result<AcessoAluno>.Failure(notifications);
        return Result<AcessoAluno>.Success(new AcessoAluno(id, aluno!.Id, dataHora, null));
    }

    // Usado pelo repositório para reconstruir registros já gravados (sem revalidar horário)
    public static AcessoAluno Reconstituir(int id, int alunoId, DateTime entrada, DateTime? saida)
        => new(id, alunoId, entrada, saida);

    public Result<AcessoAluno> RegistrarSaida(DateTime saida)
    {
        if (DataHoraSaida.HasValue)
            return Result<AcessoAluno>.Failure("DataHoraSaida", "SAIDA_JA_REGISTRADA");
        if (saida < DataHora)
            return Result<AcessoAluno>.Failure("DataHoraSaida", "SAIDA_ANTERIOR_A_ENTRADA");
        DataHoraSaida = saida;
        return Result<AcessoAluno>.Success(this);
    }
}