using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class MatriculaService : IMatriculaService
{
    private readonly Func<IMatriculaRepository> _repositoryFactory;
    private readonly IAlunoService _alunoService;

    public MatriculaService(Func<IMatriculaRepository> repositoryFactory, IAlunoService alunoService)
    {
        _repositoryFactory = repositoryFactory;
        _alunoService = alunoService;
    }

    public async Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        var entity = await repository.ObterPorId(id, cancellationToken);
        return entity is null ? null : await MapToDtoAsync(entity, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        var entities = await repository.ObterTodos(cancellationToken);
        return await MapToDtosAsync(entities, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        var entities = await repository.ObterPorAluno(alunoId, cancellationToken);
        return await MapToDtosAsync(entities, cancellationToken);
    }

    public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        var entity = await repository.ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        return entity is null ? null : await MapToDtoAsync(entity, cancellationToken);
    }

    public async Task<bool> PossuiMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        return await repository.PossuiMatriculaAtiva(alunoId, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterAtivasAsync(int alunoId = 0, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        var entities = await repository.ObterAtivas(alunoId, cancellationToken);
        return await MapToDtosAsync(entities, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int dias, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        var entities = await repository.ObterVencendoEmDias(dias, cancellationToken);
        return await MapToDtosAsync(entities, cancellationToken);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        // ATENÇÃO: assume que Domain.Enums.MatriculaPlano tem os mesmos valores numéricos que AppMatriculaPlano.
        var entities = await repository.ObterPorPlano((Domain.Enums.MatriculaPlano)(int)plano, cancellationToken);
        return await MapToDtosAsync(entities, cancellationToken);
    }

    private async Task<IEnumerable<MatriculaDto>> MapToDtosAsync(IEnumerable<Matricula> entities, CancellationToken cancellationToken)
    {
        var tasks = entities.Select(e => MapToDtoAsync(e, cancellationToken));
        return await Task.WhenAll(tasks);
    }

    // Usamos o IAlunoService (já confiável, pois veio pronto no projeto) para buscar os dados
    // do aluno vinculado, em vez de depender de uma navegação Matricula.Aluno que não existe
    // na entidade (ela só guarda o AlunoId).
    private async Task<MatriculaDto> MapToDtoAsync(Matricula entity, CancellationToken cancellationToken)
    {
        AlunoDto? aluno = entity.AlunoId > 0
            ? await _alunoService.ObterPorIdAsync(entity.AlunoId, cancellationToken)
            : null;

        return new MatriculaDto
        {
            Id = entity.Id,
            AlunoMatricula = aluno,
            Plano = (AppMatriculaPlano)(int)entity.Plano,
            DataInicio = entity.DataInicio,
            DataFim = entity.DataFim,
            Objetivo = entity.Objetivo,
            RestricoesMedicas = (AppMatriculaRestricoes)(int)entity.RestricoesMedicas,
            ObservacoesRestricoes = entity.ObservacoesRestricoes,
            LaudoMedico = entity.LaudoMedico is null ? null : new ArquivoDto { Conteudo = entity.LaudoMedico.Conteudo }
        };
    }
}