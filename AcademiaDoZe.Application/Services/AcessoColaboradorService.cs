
﻿// Jonathan de Souza Pereira

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository> _repoFactory;

    public AcessoColaboradorService(
        Func<IAcessoColaboradorRepository> repoFactory)
    {
        _repoFactory = repoFactory;
    }

    public async Task<AcessoColaboradorDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var acesso = await repo.ObterPorId(
            id,
            cancellationToken);

        return acesso == null
            ? null
            : MapToDto(acesso);
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var acessos = await repo.ObterTodos(
            cancellationToken);

        return acessos.Select(MapToDto);
    }

    public async Task<AcessoColaboradorDto> RegistrarEntradaAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "O registro de entrada depende da implementação do repositório de AcessoColaborador e da validação das regras de jornada.");
    }

    public async Task<AcessoColaboradorDto> RegistrarSaidaAsync(
        int acessoId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "O registro de saída depende da implementação da regra de persistência para saída do colaborador.");
    }

    public async Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        return await repo.Remover(
            id,
            cancellationToken);
    }

    public async Task<IEnumerable<AcessoColaboradorDto>> ObterPorColaboradorAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var acessos = await repo.ObterAcessosPorColaboradorPeriodo(
            colaboradorId: colaboradorId,
            inicio: null,
            fim: null,
            cancellationToken: cancellationToken);

        return acessos.Select(MapToDto);
    }

    public async Task<TimeSpan> ObterTotalHorasTrabalhadasHojeAsync(
        int colaboradorId,
        CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var hoje = DateOnly.FromDateTime(DateTime.Today);

        return await repo.ObterHorasTrabalhadasNoDia(
            colaboradorId,
            hoje,
            cancellationToken);
    }

    private static AcessoColaboradorDto MapToDto(
        AcessoColaborador acesso)
    {
        return new AcessoColaboradorDto
        {
            Id = acesso.Id,
            Entrada = acesso.DataHora
        };
    }
}

