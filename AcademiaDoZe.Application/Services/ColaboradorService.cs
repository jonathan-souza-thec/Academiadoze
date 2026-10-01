using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Application.Services;

public class ColaboradorService : IColaboradorService
{
    public Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<ColaboradorDto> AdicionarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<ColaboradorDto> AtualizarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<ColaboradorDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<ColaboradorDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo tipo, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}