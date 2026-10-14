// Jonathan de Souza Pereira

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class ColaboradorService : IColaboradorService
{
    private readonly Func<IColaboradorRepository> _colaboradorRepositoryFactory;
    private readonly Func<ILogradouroRepository> _logradouroRepositoryFactory;

    public ColaboradorService(
        Func<IColaboradorRepository> colaboradorRepositoryFactory,
        Func<ILogradouroRepository> logradouroRepositoryFactory)
    {
        _colaboradorRepositoryFactory = colaboradorRepositoryFactory;
        _logradouroRepositoryFactory = logradouroRepositoryFactory;
    }

    // ── Consultas ─────────────────────────────────────────────────────────────

    public async Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepositoryFactory().ObterPorId(id, cancellationToken);
        return colaborador is null ? null : await ParaDtoAsync(colaborador, cancellationToken);
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var colaboradores = await _colaboradorRepositoryFactory().ObterTodos(cancellationToken);
        return await ParaDtosAsync(colaboradores, cancellationToken);
    }

    public async Task<ColaboradorDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) return null;
        var colaborador = await _colaboradorRepositoryFactory().ObterPorCpf(cpfResult.Value!, cancellationToken);
        return colaborador is null ? null : await ParaDtoAsync(colaborador, cancellationToken);
    }

    public async Task<ColaboradorDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) return null;
        var colaborador = await _colaboradorRepositoryFactory().ObterPorEmail(emailResult.Value!, cancellationToken);
        return colaborador is null ? null : await ParaDtoAsync(colaborador, cancellationToken);
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        var colaboradores = await _colaboradorRepositoryFactory().ObterPorTipo(tipo.ToDomain(), cancellationToken);
        return await ParaDtosAsync(colaboradores, cancellationToken);
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        var colaboradores = await _colaboradorRepositoryFactory().ObterPorVinculo(vinculo.ToDomain(), cancellationToken);
        return await ParaDtosAsync(colaboradores, cancellationToken);
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) return false;
        return await _colaboradorRepositoryFactory().CpfJaExiste(cpfResult.Value!, id, cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) return false;
        return await _colaboradorRepositoryFactory().EmailJaExiste(emailResult.Value!, id, cancellationToken);
    }

    // ── Escrita ───────────────────────────────────────────────────────────────

    public async Task<ColaboradorDto> AdicionarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);
        var colaboradorRepo = _colaboradorRepositoryFactory();
        var logradouroRepo = _logradouroRepositoryFactory();

        var logradouroId = colaboradorDto.Endereco?.Id ?? 0;
        var logradouro = await logradouroRepo.ObterPorId(logradouroId, cancellationToken)
            ?? throw new InvalidOperationException($"Logradouro com ID {logradouroId} não encontrado.");

        var cpfResult = Cpf.Criar(colaboradorDto.Cpf);
        if (cpfResult.IsSuccess && await colaboradorRepo.CpfJaExiste(cpfResult.Value!, null, cancellationToken))
            throw new InvalidOperationException("Já existe um colaborador cadastrado com este CPF.");

        var emailResult = Email.Criar(colaboradorDto.Email ?? string.Empty);
        if (emailResult.IsSuccess && await colaboradorRepo.EmailJaExiste(emailResult.Value!, null, cancellationToken))
            throw new InvalidOperationException("Já existe um colaborador cadastrado com este e-mail.");

        // valida a senha em texto puro antes de gerar o hash
        var senhaValidacao = Senha.Criar(colaboradorDto.Senha ?? string.Empty);
        if (senhaValidacao.IsFailure)
            throw new InvalidOperationException($"Erro de validação ao converter Colaborador: {string.Join(", ", senhaValidacao.Notifications.Select(n => n.Mensagem))}");

        var dtoComHash = ClonarComSenha(colaboradorDto, PasswordHasher.Hash(colaboradorDto.Senha!));
        var entity = dtoComHash.ToEntity(logradouro);
        var adicionado = await colaboradorRepo.Adicionar(entity, cancellationToken);
        return adicionado.ToDto(logradouro);
    }

    public async Task<ColaboradorDto> AtualizarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);
        var colaboradorRepo = _colaboradorRepositoryFactory();
        var logradouroRepo = _logradouroRepositoryFactory();

        var existente = await colaboradorRepo.ObterPorId(colaboradorDto.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Colaborador com ID {colaboradorDto.Id} não encontrado.");

        var logradouroId = colaboradorDto.Endereco?.Id ?? 0;
        var logradouro = await logradouroRepo.ObterPorId(logradouroId, cancellationToken)
            ?? throw new InvalidOperationException($"Logradouro com ID {logradouroId} não encontrado.");

        var cpfResult = Cpf.Criar(colaboradorDto.Cpf);
        if (cpfResult.IsSuccess && await colaboradorRepo.CpfJaExiste(cpfResult.Value!, colaboradorDto.Id, cancellationToken))
            throw new InvalidOperationException("Já existe outro colaborador cadastrado com este CPF.");

        var emailResult = Email.Criar(colaboradorDto.Email ?? string.Empty);
        if (emailResult.IsSuccess && await colaboradorRepo.EmailJaExiste(emailResult.Value!, colaboradorDto.Id, cancellationToken))
            throw new InvalidOperationException("Já existe outro colaborador cadastrado com este e-mail.");

        // Senha: se uma nova senha foi informada, valida e gera novo hash; senão mantém o hash atual
        string senhaParaPersistir = existente.Senha.Valor;
        if (!string.IsNullOrWhiteSpace(colaboradorDto.Senha))
        {
            var senhaValidacao = Senha.Criar(colaboradorDto.Senha);
            if (senhaValidacao.IsFailure)
                throw new InvalidOperationException($"Erro de validação da senha: {string.Join(", ", senhaValidacao.Notifications.Select(n => n.Mensagem))}");
            senhaParaPersistir = PasswordHasher.Hash(colaboradorDto.Senha);
        }

        var dtoComSenha = ClonarComSenha(colaboradorDto, senhaParaPersistir);
        var atualizado = existente.UpdateFromDto(dtoComSenha, logradouro);
        var resultado = await colaboradorRepo.Atualizar(atualizado, cancellationToken);
        return resultado.ToDto(logradouro);
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        => await _colaboradorRepositoryFactory().Remover(id, cancellationToken);

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        var senhaValidacao = Senha.Criar(novaSenha);
        if (senhaValidacao.IsFailure)
            throw new InvalidOperationException($"Erro de validação da nova senha: {string.Join(", ", senhaValidacao.Notifications.Select(n => n.Mensagem))}");

        var senhaHashResult = Senha.Criar(PasswordHasher.Hash(novaSenha));
        if (senhaHashResult.IsFailure)
            throw new InvalidOperationException("Erro ao gerar o hash da nova senha.");

        return await _colaboradorRepositoryFactory().TrocarSenha(id, senhaHashResult.Value!, cancellationToken);
    }

    // ── Auxiliares ────────────────────────────────────────────────────────────

    // A entidade guarda apenas o LogradouroId; o DTO precisa do logradouro completo para exibir/editar o endereço.
    private async Task<ColaboradorDto> ParaDtoAsync(Colaborador colaborador, CancellationToken cancellationToken)
    {
        var logradouro = await _logradouroRepositoryFactory().ObterPorId(colaborador.Endereco.LogradouroId, cancellationToken);
        return colaborador.ToDto(logradouro);
    }

    // Versão para listas: busca cada logradouro uma única vez (evita N consultas repetidas).
    private async Task<List<ColaboradorDto>> ParaDtosAsync(IEnumerable<Colaborador> colaboradores, CancellationToken cancellationToken)
    {
        var logradouroRepo = _logradouroRepositoryFactory();
        var cache = new Dictionary<int, Logradouro?>();
        var resultado = new List<ColaboradorDto>();
        foreach (var colaborador in colaboradores)
        {
            var logradouroId = colaborador.Endereco.LogradouroId;
            if (!cache.TryGetValue(logradouroId, out var logradouro))
            {
                logradouro = await logradouroRepo.ObterPorId(logradouroId, cancellationToken);
                cache[logradouroId] = logradouro;
            }
            resultado.Add(colaborador.ToDto(logradouro));
        }
        return resultado;
    }

    private static ColaboradorDto ClonarComSenha(ColaboradorDto origem, string senha) => new()
    {
        Id = origem.Id,
        Nome = origem.Nome,
        Cpf = origem.Cpf,
        DataNascimento = origem.DataNascimento,
        Telefone = origem.Telefone,
        Email = origem.Email,
        Endereco = origem.Endereco,
        Numero = origem.Numero,
        Complemento = origem.Complemento,
        Senha = senha,
        Foto = origem.Foto,
        DataAdmissao = origem.DataAdmissao,
        Tipo = origem.Tipo,
        Vinculo = origem.Vinculo
    };
}