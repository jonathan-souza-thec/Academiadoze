
﻿// Jonathan de Souza Pereira

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _repoFactory;

    public AcessoAlunoService(Func<IAcessoAlunoRepository> repoFactory)
    {
        _repoFactory = repoFactory;
    }

    public async Task<AcessoAlunoDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var acesso = await repo.ObterPorId(id, cancellationToken);

        return acesso == null
            ? null
            : MapToDto(acesso);
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var acessos = await repo.ObterTodos(cancellationToken);

        return acessos.Select(MapToDto);
    }

    public async Task<AcessoAlunoDto> RegistrarEntradaAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "O registro de entrada depende da implementação do repositório de AcessoAluno e da validação da matrícula ativa.");
    }

    public async Task<AcessoAlunoDto> RegistrarSaidaAsync(
        int acessoId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "O registro de saída depende de uma propriedade de saída no domínio ou da regra de persistência correspondente.");
    }

    public async Task<bool> RemoverAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        return await repo.Remover(id, cancellationToken);
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterPorAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var acessos = await repo.ObterAcessosPorAlunoPeriodo(
            alunoId: alunoId,
            inicio: null,
            fim: null,
            cancellationToken: cancellationToken);

        return acessos.Select(MapToDto);
    }

    private static AcessoAlunoDto MapToDto(
        Domain.Entities.AcessoAluno acesso)
    {
        return new AcessoAlunoDto
        {
            Id = acesso.Id,
            Entrada = acesso.DataHora
        };
    }
}
