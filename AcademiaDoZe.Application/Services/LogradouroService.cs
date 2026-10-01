using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class LogradouroService : ILogradouroService
{
    private readonly Func<ILogradouroRepository> _repositoryFactory;

    public LogradouroService(Func<ILogradouroRepository> repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task<LogradouroDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        var entity = await repository.ObterPorId(id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<LogradouroDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        var entities = await repository.ObterTodos(cancellationToken);
        return entities.Select(MapToDto);
    }

    public async Task<LogradouroDto> AdicionarAsync(LogradouroDto logradouroDto, CancellationToken cancellationToken = default)
    {
        var resultado = Logradouro.Criar(
            id: 0,
            cep: logradouroDto.Cep,
            nome: logradouroDto.Nome,
            bairro: logradouroDto.Bairro,
            cidade: logradouroDto.Cidade,
            estado: logradouroDto.Estado,
            pais: logradouroDto.Pais);

        if (resultado.IsFailure)
            throw new ArgumentException(string.Join(", ", resultado.Notifications.Select(n => n.Mensagem)));

        var repository = _repositoryFactory();
        var salvo = await repository.Adicionar(resultado.Value!, cancellationToken);
        return MapToDto(salvo);
    }

    public async Task<LogradouroDto> AtualizarAsync(LogradouroDto logradouroDto, CancellationToken cancellationToken = default)
    {
        var resultado = Logradouro.Criar(
            id: logradouroDto.Id,
            cep: logradouroDto.Cep,
            nome: logradouroDto.Nome,
            bairro: logradouroDto.Bairro,
            cidade: logradouroDto.Cidade,
            estado: logradouroDto.Estado,
            pais: logradouroDto.Pais);

        if (resultado.IsFailure)
            throw new ArgumentException(string.Join(", ", resultado.Notifications.Select(n => n.Mensagem)));

        var repository = _repositoryFactory();
        var atualizado = await repository.Atualizar(resultado.Value!, cancellationToken);
        return MapToDto(atualizado);
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        return await repository.Remover(id, cancellationToken);
    }

    public async Task<LogradouroDto?> ObterPorCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        // ATENÇÃO: assume que Cep tem um método estático Criar(string) => Result<Cep>.
        var cepResultado = Cep.Criar(cep);
        if (cepResultado.IsFailure)
            return null;

        var repository = _repositoryFactory();
        var entity = await repository.ObterPorCep(cepResultado.Value!, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorCidadeAsync(string cidade, CancellationToken cancellationToken = default)
    {
        var repository = _repositoryFactory();
        var entities = await repository.ObterPorCidade(cidade, cancellationToken);
        return entities.Select(MapToDto);
    }

    private static LogradouroDto MapToDto(Logradouro entity) => new()
    {
        Id = entity.Id,
        Cep = entity.Cep.Valor,
        Nome = entity.Nome,
        Bairro = entity.Bairro,
        Cidade = entity.Cidade,
        Estado = entity.Estado,
        Pais = entity.Pais
    };
}