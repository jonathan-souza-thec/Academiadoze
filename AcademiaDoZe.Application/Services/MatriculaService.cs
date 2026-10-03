// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;      // CORREÇÃO 1: necessário para o tipo Matricula nos helpers
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class MatriculaService : IMatriculaService
{
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;

    public MatriculaService(
        Func<IMatriculaRepository> matriculaRepoFactory,
        Func<IAlunoRepository> alunoRepoFactory)
    {
        _matriculaRepoFactory = matriculaRepoFactory;
        _alunoRepoFactory = alunoRepoFactory;
    }

    // CORREÇÃO 2: helpers novos. A entidade Matricula só tem AlunoId (não tem objeto Aluno),
    // então o aluno é buscado no repositório e convertido para DTO aqui, num único lugar.
    private async Task<MatriculaDto> ToDtoAsync(Matricula matricula, CancellationToken ct)
    {
        var aluno = await _alunoRepoFactory().ObterPorId(matricula.AlunoId, ct)
            ?? throw new InvalidOperationException($"Aluno com ID {matricula.AlunoId} não encontrado.");
        return matricula.ToDto(aluno.ToDto());
    }

    private async Task<IEnumerable<MatriculaDto>> ToDtoAsync(IEnumerable<Matricula> lista, CancellationToken ct)
    {
        var resultado = new List<MatriculaDto>();
        foreach (var m in lista)
            resultado.Add(await ToDtoAsync(m, ct));
        return resultado;
    }

    // CORREÇÃO 3 (era linha 28): matricula.AlunoMatricula -> helper
    public async Task<MatriculaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var matricula = await _matriculaRepoFactory().ObterPorId(id, cancellationToken);
        return matricula is null ? null : await ToDtoAsync(matricula, cancellationToken);
    }

    // CORREÇÃO 4 (era linha 36)
    public async Task<IEnumerable<MatriculaDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var lista = await _matriculaRepoFactory().ObterTodos(cancellationToken);
        return await ToDtoAsync(lista, cancellationToken);
    }

    // Sem alteração (usa matriculaDto.AlunoMatricula, que existe no DTO)
    public async Task<MatriculaDto> AdicionarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);

        var alunoRepo = _alunoRepoFactory();
        var matriculaRepo = _matriculaRepoFactory();

        var alunoId = matriculaDto.AlunoMatricula?.Id
            ?? throw new InvalidOperationException("Aluno é obrigatório para criar uma matrícula.");

        var aluno = await alunoRepo.ObterPorId(alunoId, cancellationToken)
            ?? throw new InvalidOperationException($"Aluno com ID {alunoId} não encontrado.");

        // Regra: não permitir nova matrícula se já houver matrícula ativa
        if (await matriculaRepo.PossuiMatriculaAtiva(alunoId, cancellationToken))
            throw new InvalidOperationException("O aluno já possui uma matrícula ativa.");

        var entity = matriculaDto.ToEntity(aluno);
        var adicionada = await matriculaRepo.Adicionar(entity, cancellationToken);
        return adicionada.ToDto(aluno.ToDto());
    }

    public async Task<MatriculaDto> AtualizarAsync(MatriculaDto matriculaDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(matriculaDto);

        var alunoRepo = _alunoRepoFactory();
        var matriculaRepo = _matriculaRepoFactory();

        var existente = await matriculaRepo.ObterPorId(matriculaDto.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Matrícula com ID {matriculaDto.Id} não encontrada.");

        // CORREÇÃO 5 (era linha 71): existente.AlunoMatricula.Id -> existente.AlunoId
        var alunoId = matriculaDto.AlunoMatricula?.Id ?? existente.AlunoId;
        var aluno = await alunoRepo.ObterPorId(alunoId, cancellationToken)
            ?? throw new InvalidOperationException($"Aluno com ID {alunoId} não encontrado.");

        var atualizada = existente.UpdateFromDto(matriculaDto, aluno);
        var resultado = await matriculaRepo.Atualizar(atualizada, cancellationToken);
        return resultado.ToDto(aluno.ToDto());
    }

    // Sem alteração
    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var repo = _matriculaRepoFactory();
        return await repo.Remover(id, cancellationToken);
    }

    // CORREÇÃO 6 (era linha 90)
    public async Task<IEnumerable<MatriculaDto>> ObterPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var lista = await _matriculaRepoFactory().ObterPorAluno(alunoId, cancellationToken);
        return await ToDtoAsync(lista, cancellationToken);
    }

    // CORREÇÃO 7 (era linha 98)
    public async Task<MatriculaDto?> ObterMatriculaAtivaPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var matricula = await _matriculaRepoFactory().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
        return matricula is null ? null : await ToDtoAsync(matricula, cancellationToken);
    }

    // Sem alteração
    public async Task<bool> PossuiMatriculaAtivaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var repo = _matriculaRepoFactory();
        return await repo.PossuiMatriculaAtiva(alunoId, cancellationToken);
    }

    // CORREÇÃO 8 (era linha 111)
    public async Task<IEnumerable<MatriculaDto>> ObterPorPlanoAsync(AppMatriculaPlano plano, CancellationToken cancellationToken = default)
    {
        var lista = await _matriculaRepoFactory().ObterPorPlano(plano.ToDomain(), cancellationToken);
        return await ToDtoAsync(lista, cancellationToken);
    }

    // CORREÇÃO 9 (era linha 118)
    public async Task<IEnumerable<MatriculaDto>> ObterVencendoEmDiasAsync(int dias, CancellationToken cancellationToken = default)
    {
        var lista = await _matriculaRepoFactory().ObterVencendoEmDias(dias, cancellationToken);
        return await ToDtoAsync(lista, cancellationToken);
    }
}