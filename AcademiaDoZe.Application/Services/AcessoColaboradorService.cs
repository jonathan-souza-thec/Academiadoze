// Jonathan de Souza Pereira

using AcademiaDoZe.Application.Common;
using AcademiaDoZe.Application.DTOs;
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

    public async Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var acesso = await _repoFactory().ObterPorId(id, cancellationToken);
        if (acesso == null) return null;
        return (await ParaDtosAsync([acesso], cancellationToken)).First();
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterTodos(cancellationToken);
        return await ParaDtosAsync(acessos, cancellationToken);
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterPorColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterAcessosPorColaboradorPeriodo(colaboradorId, null, null, cancellationToken);
        return await ParaDtosAsync(acessos, cancellationToken);
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterDeHojeAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        var acessos = await _repoFactory().ObterAcessosPorColaboradorPeriodo(null, hoje, hoje, cancellationToken);
        return await ParaDtosAsync(acessos, cancellationToken);
    }

    public async Task<AcessoColaboradorDto?> ObterAcessoAbertoAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var ultimo = await _repoFactory().ObterUltimoAcesso(colaboradorId, cancellationToken);
        if (ultimo is not { EstaDentro: true }) return null;
        return (await ParaDtosAsync([ultimo], cancellationToken)).First();
    }

    public async Task<TimeSpan> ObterTotalHorasTrabalhadasHojeAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        return await _repoFactory().ObterHorasTrabalhadasNoDia(colaboradorId, hoje, cancellationToken);
    }

    public async Task<AcessoColaboradorDto> RegistrarEntradaAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var colaborador = await _colaboradorRepoFactory().ObterPorId(colaboradorId, cancellationToken)
            ?? throw new InvalidOperationException($"Colaborador com ID {colaboradorId} não encontrado.");

        var agora = DateTime.Now;
        var hoje = DateOnly.FromDateTime(agora);
        var dto = new AcessoColaboradorDto { Colaborador = colaborador.ToDto(), Entrada = agora };

        var ultimo = await repo.ObterUltimoAcesso(colaboradorId, cancellationToken);
        if (ultimo is { EstaDentro: true })
            return Negar(dto, "Acesso negado: o colaborador já possui uma entrada em aberto. Registre a saída primeiro.");

        // Regra: limite de 8h (CLT) ou 6h (estágio), somando todos os registros do dia
        var horasHoje = await repo.ObterHorasTrabalhadasNoDia(colaboradorId, hoje, cancellationToken);
        var limite = colaborador.LimiteHorasDiarias;
        if (horasHoje >= limite)
        {
            string vinculo = colaborador.Vinculo == ColaboradorVinculo.CLT ? "CLT" : "estágio";
            dto.TotalHorasDia = horasHoje;
            return Negar(dto,
                $"Acesso negado: o colaborador já atingiu o limite diário de {limite.ToHoraMinutoSegundo()} ({vinculo}). " +
                $"Horas trabalhadas hoje: {horasHoje.ToHoraMinutoSegundo()}.");
        }

        var criar = AcessoColaborador.Criar(0, colaborador, agora);
        if (criar.IsFailure)
            return Negar(dto, "Acesso negado: fora do horário de funcionamento (06:00 às 22:00).");

        var salvo = await repo.Adicionar(criar.Value!, cancellationToken);

        dto.Id = salvo.Id;
        dto.Autorizado = true;
        dto.TotalHorasDia = horasHoje;
        dto.Mensagem = $"Entrada registrada. Horas trabalhadas hoje até o momento: {horasHoje.ToHoraMinutoSegundo()}.";
        return dto;
    }

    public async Task<AcessoColaboradorDto> RegistrarSaidaAsync(int acessoId, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var acesso = await repo.ObterPorId(acessoId, cancellationToken)
            ?? throw new InvalidOperationException($"Acesso com ID {acessoId} não encontrado.");

        var colaborador = await _colaboradorRepoFactory().ObterPorId(acesso.ColaboradorId, cancellationToken);
        var dto = new AcessoColaboradorDto
        {
            Id = acesso.Id,
            Colaborador = colaborador?.ToDto(),
            Entrada = acesso.DataHora,
            Saida = acesso.DataHoraSaida
        };

        var resultado = acesso.RegistrarSaida(DateTime.Now);
        if (resultado.IsFailure)
            return Negar(dto, "Saída não registrada: este acesso já possui saída.");

        await repo.Atualizar(acesso, cancellationToken);

        // Regra: na saída, somar todos os registros do dia
        var totalDia = await repo.ObterHorasTrabalhadasNoDia(
            acesso.ColaboradorId, DateOnly.FromDateTime(acesso.DataHora), cancellationToken);

        dto.Saida = acesso.DataHoraSaida;
        dto.Autorizado = true;
        dto.TotalHorasDia = totalDia;
        dto.Mensagem =
            $"Saída registrada. Permanência neste registro: {acesso.TempoPermanencia!.Value.ToHoraMinutoSegundo()}. " +
            $"Total no dia (soma de todos os registros): {totalDia.ToHoraMinutoSegundo()}.";
        return dto;
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        => await _repoFactory().Remover(id, cancellationToken);

    private static AcessoColaboradorDto Negar(AcessoColaboradorDto dto, string mensagem)
    {
        dto.Autorizado = false;
        dto.Mensagem = mensagem;
        return dto;
    }

    private async Task<List<AcessoColaboradorDto>> ParaDtosAsync(IEnumerable<AcessoColaborador> acessos, CancellationToken cancellationToken)
    {
        var colaboradorRepo = _colaboradorRepoFactory();
        var cache = new Dictionary<int, ColaboradorDto?>();
        var lista = new List<AcessoColaboradorDto>();

        foreach (var a in acessos)
        {
            if (!cache.TryGetValue(a.ColaboradorId, out var colaboradorDto))
            {
                var colaborador = await colaboradorRepo.ObterPorId(a.ColaboradorId, cancellationToken);
                colaboradorDto = colaborador?.ToDto();
                cache[a.ColaboradorId] = colaboradorDto;
            }

            lista.Add(new AcessoColaboradorDto
            {
                Id = a.Id,
                Colaborador = colaboradorDto,
                Entrada = a.DataHora,
                Saida = a.DataHoraSaida
            });
        }
        return lista;
    }
}