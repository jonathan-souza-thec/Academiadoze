// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Exceptions;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository> _repoFactory;
    private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;

    public AcessoColaboradorService(
        Func<IAcessoColaboradorRepository> repoFactory,
        Func<IColaboradorRepository> colaboradorRepoFactory)
    {
        _repoFactory = repoFactory;
        _colaboradorRepoFactory = colaboradorRepoFactory;
    }

    // Limite diário: 8 horas se CLT, 6 horas se estágio
    private static TimeSpan LimiteDiario(Colaborador colaborador)
        => colaborador.Vinculo == ColaboradorVinculo.CLT ? TimeSpan.FromHours(8) : TimeSpan.FromHours(6);

    public async Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();
        var acesso = await repo.ObterPorId(id, cancellationToken);
        if (acesso == null) return null;

        var dia = DateOnly.FromDateTime(acesso.DataHora);
        var doDia = await repo.ObterAcessosPorColaboradorPeriodo(acesso.ColaboradorId, dia, dia, cancellationToken);
        var visita = AcessoPareamento
            .Parear(doDia.Select(a => new AcessoEvento(a.Id, a.DataHora)))
            .FirstOrDefault(v => v.Id == id || v.SaidaId == id);
        if (visita == null) return null;

        var colaborador = await _colaboradorRepoFactory().ObterPorId(acesso.ColaboradorId, cancellationToken);
        if (colaborador == null) return null;

        return ToDto(visita, colaborador.ToDto());
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterTodos(cancellationToken);
        return await ToDtosAsync(acessos, cancellationToken);
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterDoDiaAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var acessos = await _repoFactory().ObterAcessosPorColaboradorPeriodo(null, hoje, hoje, cancellationToken);
        return await ToDtosAsync(acessos, cancellationToken);
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterPorColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterAcessosPorColaboradorPeriodo(colaboradorId, null, null, cancellationToken);
        return await ToDtosAsync(acessos, cancellationToken);
    }

    public async Task<TimeSpan> ObterTotalHorasTrabalhadasHojeAsync(int colaboradorId, CancellationToken cancellationToken = default)
        => await _repoFactory().ObterHorasTrabalhadasNoDia(colaboradorId, DateOnly.FromDateTime(DateTime.Today), cancellationToken);

    public async Task<bool> EstaNaAcademiaAsync(int colaboradorId, CancellationToken cancellationToken = default)
        => await _repoFactory().EstaNaAcademia(colaboradorId, cancellationToken);

    public async Task<AcessoColaboradorDto> RegistrarEntradaAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var colaborador = await _colaboradorRepoFactory().ObterPorId(colaboradorId, cancellationToken)
            ?? throw new InvalidOperationException($"Colaborador com ID {colaboradorId} não encontrado.");

        if (await repo.EstaNaAcademia(colaboradorId, cancellationToken))
            throw new AcessoNegadoException("Acesso negado: o colaborador já está dentro da academia. Registre a saída.");

        // Regra: validar se já não ultrapassa o limite (8h CLT / 6h estágio)
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var totalHoje = await repo.ObterHorasTrabalhadasNoDia(colaboradorId, hoje, cancellationToken);
        var limite = LimiteDiario(colaborador);
        if (totalHoje >= limite)
            throw new AcessoNegadoException(
                $"Acesso negado: limite diário de {limite.TotalHours:0} horas já atingido ({AcessoPareamento.Formatar(totalHoje)} trabalhadas hoje).");

        var criado = AcessoColaborador.Criar(0, colaborador, AcessoPareamento.Agora());
        if (criado.IsFailure)
            throw new AcessoNegadoException("Acesso negado: " + AcessoPareamento.TraduzirNotificacoes(criado.Notifications));

        var adicionado = await repo.Adicionar(criado.Value!, cancellationToken);

        return new AcessoColaboradorDto
        {
            Id = adicionado.Id,
            Colaborador = colaborador.ToDto(),
            Entrada = adicionado.DataHora,
            TotalHorasHoje = totalHoje,
            Mensagem = $"Entrada registrada. Horas trabalhadas hoje até o momento: {AcessoPareamento.Formatar(totalHoje)}."
        };
    }

    public async Task<AcessoColaboradorDto> RegistrarSaidaAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var colaborador = await _colaboradorRepoFactory().ObterPorId(colaboradorId, cancellationToken)
            ?? throw new InvalidOperationException($"Colaborador com ID {colaboradorId} não encontrado.");

        var ultimo = await repo.ObterUltimoAcesso(colaboradorId, cancellationToken);
        if (ultimo == null || !await repo.EstaNaAcademia(colaboradorId, cancellationToken))
            throw new AcessoNegadoException("Saída não registrada: o colaborador não possui entrada em aberto hoje.");

        var saida = AcessoPareamento.Agora();
        if (saida < ultimo.DataHora) saida = ultimo.DataHora;
        // a academia fecha às 22h: saídas após esse horário são registradas no fechamento
        var fechamento = saida.Date.AddHours(22);
        if (saida > fechamento) saida = fechamento < ultimo.DataHora ? ultimo.DataHora : fechamento;

        var criado = AcessoColaborador.Criar(0, colaborador, saida);
        if (criado.IsFailure)
            throw new AcessoNegadoException("Saída não registrada: " + AcessoPareamento.TraduzirNotificacoes(criado.Notifications));

        var adicionado = await repo.Adicionar(criado.Value!, cancellationToken);

        // Regra: na saída, somar todos os registros do dia
        var permanencia = adicionado.DataHora - ultimo.DataHora;
        var totalDia = await repo.ObterHorasTrabalhadasNoDia(colaboradorId, DateOnly.FromDateTime(adicionado.DataHora), cancellationToken);
        var limite = LimiteDiario(colaborador);

        var mensagem = $"Saída registrada. Tempo de permanência: {AcessoPareamento.Formatar(permanencia)}. " +
                       $"Total de horas hoje (soma de todos os registros): {AcessoPareamento.Formatar(totalDia)}.";
        if (totalDia > limite)
            mensagem += $" Atenção: limite diário de {limite.TotalHours:0} horas excedido.";

        return new AcessoColaboradorDto
        {
            Id = ultimo.Id,
            Colaborador = colaborador.ToDto(),
            Entrada = ultimo.DataHora,
            Saida = adicionado.DataHora,
            TotalHorasHoje = totalDia,
            Mensagem = mensagem
        };
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        => await _repoFactory().Remover(id, cancellationToken);

    private static AcessoColaboradorDto ToDto(AcessoVisita visita, ColaboradorDto colaborador) => new()
    {
        Id = visita.Id,
        Colaborador = colaborador,
        Entrada = visita.Entrada,
        Saida = visita.Saida
    };

    private async Task<IEnumerable<AcessoColaboradorDto>> ToDtosAsync(IEnumerable<AcessoColaborador> acessos, CancellationToken cancellationToken)
    {
        var resultado = new List<AcessoColaboradorDto>();

        foreach (var grupo in acessos.GroupBy(a => a.ColaboradorId))
        {
            var colaborador = await _colaboradorRepoFactory().ObterPorId(grupo.Key, cancellationToken);
            if (colaborador == null) continue;

            var colaboradorDto = colaborador.ToDto();
            var visitas = AcessoPareamento.Parear(grupo.Select(a => new AcessoEvento(a.Id, a.DataHora)));
            foreach (var visita in visitas)
                resultado.Add(ToDto(visita, colaboradorDto));
        }

        return resultado.OrderByDescending(r => r.Saida ?? r.Entrada).ToList();
    }
}