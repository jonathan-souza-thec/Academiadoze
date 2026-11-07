// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Exceptions;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _repoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;

    public AcessoAlunoService(
        Func<IAcessoAlunoRepository> repoFactory,
        Func<IAlunoRepository> alunoRepoFactory,
        Func<IMatriculaRepository> matriculaRepoFactory)
    {
        _repoFactory = repoFactory;
        _alunoRepoFactory = alunoRepoFactory;
        _matriculaRepoFactory = matriculaRepoFactory;
    }

    public async Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();
        var acesso = await repo.ObterPorId(id, cancellationToken);
        if (acesso == null) return null;

        var dia = DateOnly.FromDateTime(acesso.DataHora);
        var doDia = await repo.ObterAcessosPorAlunoPeriodo(acesso.AlunoId, dia, dia, cancellationToken);
        var visita = AcessoPareamento
            .Parear(doDia.Select(a => new AcessoEvento(a.Id, a.DataHora)))
            .FirstOrDefault(v => v.Id == id || v.SaidaId == id);
        if (visita == null) return null;

        var aluno = await _alunoRepoFactory().ObterPorId(acesso.AlunoId, cancellationToken);
        if (aluno == null) return null;

        return ToDto(visita, aluno.ToDto());
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterTodos(cancellationToken);
        return await ToDtosAsync(acessos, cancellationToken);
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterDoDiaAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var acessos = await _repoFactory().ObterAcessosPorAlunoPeriodo(null, hoje, hoje, cancellationToken);
        return await ToDtosAsync(acessos, cancellationToken);
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterAcessosPorAlunoPeriodo(alunoId, null, null, cancellationToken);
        return await ToDtosAsync(acessos, cancellationToken);
    }

    public async Task<bool> EstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default)
        => await _repoFactory().EstaNaAcademia(alunoId, cancellationToken);

    public async Task<AcessoAlunoDto> RegistrarEntradaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var aluno = await _alunoRepoFactory().ObterPorId(alunoId, cancellationToken)
            ?? throw new InvalidOperationException($"Aluno com ID {alunoId} não encontrado.");

        if (await repo.EstaNaAcademia(alunoId, cancellationToken))
            throw new AcessoNegadoException("Acesso negado: o aluno já está dentro da academia. Registre a saída.");

        // Regra: validar se possui matrícula ativa
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var matricula = await _matriculaRepoFactory().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        if (matricula == null || matricula.DataInicio > hoje)
            throw new AcessoNegadoException("Acesso negado: o aluno não possui matrícula ativa.");

        var criado = AcessoAluno.Criar(0, aluno, AcessoPareamento.Agora());
        if (criado.IsFailure)
            throw new AcessoNegadoException("Acesso negado: " + AcessoPareamento.TraduzirNotificacoes(criado.Notifications));

        var adicionado = await repo.Adicionar(criado.Value!, cancellationToken);

        // Regra: na entrada, mostrar quanto tempo ainda tem de plano
        int dias = matricula.DataFim.DayNumber - hoje.DayNumber;

        return new AcessoAlunoDto
        {
            Id = adicionado.Id,
            Aluno = aluno.ToDto(),
            Entrada = adicionado.DataHora,
            DiasRestantesPlano = dias,
            Mensagem = $"Entrada autorizada. Dias restantes de plano: {dias} dia(s)."
        };
    }

    public async Task<AcessoAlunoDto> RegistrarSaidaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var aluno = await _alunoRepoFactory().ObterPorId(alunoId, cancellationToken)
            ?? throw new InvalidOperationException($"Aluno com ID {alunoId} não encontrado.");

        var ultimo = await repo.ObterUltimoAcesso(alunoId, cancellationToken);
        if (ultimo == null || !await repo.EstaNaAcademia(alunoId, cancellationToken))
            throw new AcessoNegadoException("Saída não registrada: o aluno não possui entrada em aberto hoje.");

        var saida = AcessoPareamento.Agora();
        if (saida < ultimo.DataHora) saida = ultimo.DataHora;
        // a academia fecha às 22h: saídas após esse horário são registradas no fechamento
        var fechamento = saida.Date.AddHours(22);
        if (saida > fechamento) saida = fechamento < ultimo.DataHora ? ultimo.DataHora : fechamento;

        var criado = AcessoAluno.Criar(0, aluno, saida);
        if (criado.IsFailure)
            throw new AcessoNegadoException("Saída não registrada: " + AcessoPareamento.TraduzirNotificacoes(criado.Notifications));

        var adicionado = await repo.Adicionar(criado.Value!, cancellationToken);

        // Regra: na saída, mostrar o tempo que permaneceu na academia
        var permanencia = adicionado.DataHora - ultimo.DataHora;

        return new AcessoAlunoDto
        {
            Id = ultimo.Id,
            Aluno = aluno.ToDto(),
            Entrada = ultimo.DataHora,
            Saida = adicionado.DataHora,
            Mensagem = $"Saída registrada. Tempo de permanência: {AcessoPareamento.Formatar(permanencia)}."
        };
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        => await _repoFactory().Remover(id, cancellationToken);

    private static AcessoAlunoDto ToDto(AcessoVisita visita, AlunoDto aluno) => new()
    {
        Id = visita.Id,
        Aluno = aluno,
        Entrada = visita.Entrada,
        Saida = visita.Saida
    };

    private async Task<IEnumerable<AcessoAlunoDto>> ToDtosAsync(IEnumerable<AcessoAluno> acessos, CancellationToken cancellationToken)
    {
        var resultado = new List<AcessoAlunoDto>();

        foreach (var grupo in acessos.GroupBy(a => a.AlunoId))
        {
            var aluno = await _alunoRepoFactory().ObterPorId(grupo.Key, cancellationToken);
            if (aluno == null) continue;

            var alunoDto = aluno.ToDto();
            var visitas = AcessoPareamento.Parear(grupo.Select(a => new AcessoEvento(a.Id, a.DataHora)));
            foreach (var visita in visitas)
                resultado.Add(ToDto(visita, alunoDto));
        }

        return resultado.OrderByDescending(r => r.Saida ?? r.Entrada).ToList();
    }
}