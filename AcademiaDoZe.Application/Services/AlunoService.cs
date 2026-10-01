
// jonathan de souza pereira 
using AcademiaDoZe.Application.DTOs;



using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class AlunoService : IAlunoService
{
    private readonly Func<IAlunoRepository> _alunoRepositoryFactory;
    private readonly Func<ILogradouroRepository> _logradouroRepositoryFactory;

    public AlunoService(Func<IAlunoRepository> alunoRepositoryFactory, Func<ILogradouroRepository> logradouroRepositoryFactory)
    {
        _alunoRepositoryFactory = alunoRepositoryFactory;
        _logradouroRepositoryFactory = logradouroRepositoryFactory;
    }

    public async Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repo = _alunoRepositoryFactory();
        var aluno = await repo.ObterPorId(id, cancellationToken);
        return aluno?.ToDto();
    }

    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var repo = _alunoRepositoryFactory();
        var alunos = await repo.ObterTodos(cancellationToken);
        return alunos.Select(a => a.ToDto());
    }

    public async Task<AlunoDto> AdicionarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);
        var alunoRepo = _alunoRepositoryFactory();
        var logradouroRepo = _logradouroRepositoryFactory();

        var logradouroId = alunoDto.Endereco?.Id ?? 0;
        var logradouro = await logradouroRepo.ObterPorId(logradouroId, cancellationToken)
            ?? throw new InvalidOperationException($"Logradouro com ID {logradouroId} não encontrado.");

        var cpfResult = Cpf.Criar(alunoDto.Cpf);
        if (cpfResult.IsSuccess && await alunoRepo.CpfJaExiste(cpfResult.Value!, null, cancellationToken))
            throw new InvalidOperationException("Já existe um aluno cadastrado com este CPF.");

        var emailResult = Email.Criar(alunoDto.Email ?? string.Empty); if (emailResult.IsSuccess && await alunoRepo.EmailJaExiste(emailResult.Value!, null, cancellationToken))
            throw new InvalidOperationException("Já existe um aluno cadastrado com este e-mail.");

        var senhaValidacao = Senha.Criar(alunoDto.Senha ?? string.Empty); if (senhaValidacao.IsFailure)
            throw new InvalidOperationException($"Erro de validação ao converter Aluno: {string.Join(", ", senhaValidacao.Notifications.Select(n => n.Mensagem))}");

        var dtoComHash = ClonarComSenha(alunoDto, PasswordHasher.Hash(alunoDto.Senha ?? string.Empty));
        var entity = dtoComHash.ToEntity(logradouro);
        var adicionado = await alunoRepo.Adicionar(entity, cancellationToken);
        return adicionado.ToDto();
    }

    public async Task<AlunoDto> AtualizarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);
        var alunoRepo = _alunoRepositoryFactory();
        var logradouroRepo = _logradouroRepositoryFactory();

        var existente = await alunoRepo.ObterPorId(alunoDto.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Aluno com ID {alunoDto.Id} não encontrado.");

        var logradouroId = alunoDto.Endereco?.Id ?? 0;
        var logradouro = await logradouroRepo.ObterPorId(logradouroId, cancellationToken)
            ?? throw new InvalidOperationException($"Logradouro com ID {logradouroId} não encontrado.");

        var cpfResult = Cpf.Criar(alunoDto.Cpf);
        if (cpfResult.IsSuccess && await alunoRepo.CpfJaExiste(cpfResult.Value!, alunoDto.Id, cancellationToken))
            throw new InvalidOperationException("Já existe outro aluno cadastrado com este CPF.");

        var emailResult = Email.Criar(alunoDto.Email ?? string.Empty); if (emailResult.IsSuccess && await alunoRepo.EmailJaExiste(emailResult.Value!, alunoDto.Id, cancellationToken))

            throw new InvalidOperationException("Já existe outro aluno cadastrado com este e-mail.");

        var dtoComSenhaAtual = ClonarComSenha(alunoDto, existente.Senha.Valor);
        var atualizado = existente.UpdateFromDto(dtoComSenhaAtual, logradouro);
        var resultado = await alunoRepo.Atualizar(atualizado, cancellationToken);
        return resultado.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var repo = _alunoRepositoryFactory();
        return await repo.Remover(id, cancellationToken);
    }

    public async Task<AlunoDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) return null;
        var repo = _alunoRepositoryFactory();
        var aluno = await repo.ObterPorCpf(cpfResult.Value!, cancellationToken);
        return aluno?.ToDto();
    }

    public async Task<AlunoDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) return null;
        var repo = _alunoRepositoryFactory();
        var aluno = await repo.ObterPorEmail(emailResult.Value!, cancellationToken);
        return aluno?.ToDto();
    }

    public async Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        var repo = _alunoRepositoryFactory();
        var alunos = await repo.ObterPorNome(nome, cancellationToken);
        return alunos.Select(a => a.ToDto());
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) return false;
        var repo = _alunoRepositoryFactory();
        return await repo.CpfJaExiste(cpfResult.Value!, id, cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) return false;
        var repo = _alunoRepositoryFactory();
        return await repo.EmailJaExiste(emailResult.Value!, id, cancellationToken);
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        var senhaValidacao = Senha.Criar(novaSenha);
        if (senhaValidacao.IsFailure)
            throw new InvalidOperationException($"Erro de validação da nova senha: {string.Join(", ", senhaValidacao.Notifications.Select(n => n.Mensagem))}");

        var senhaHashResult = Senha.Criar(PasswordHasher.Hash(novaSenha));
        if (senhaHashResult.IsFailure)
            throw new InvalidOperationException("Erro ao gerar o hash da nova senha.");

        var repo = _alunoRepositoryFactory();
        return await repo.TrocarSenha(id, senhaHashResult.Value!, cancellationToken);
    }

    private static AlunoDto ClonarComSenha(AlunoDto origem, string senha) => new()
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
        Foto = origem.Foto
    };
}