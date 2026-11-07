// Jonathan de Souza Pereira

using AcademiaDoZe.Application.Common;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _repoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;

    public AcessoAlunoService(
        Func<IAcessoAlunoRepository> repoFactory,
        Func<IAlunoRepository> alunoRepoFactory,
        Func<IMatriculaRepository> matriculaRepoFactory)
    {
        _repoFactory = repoFactory;
        _alunoRepoFactory = alunoRepoFactory;
        _matriculaRepoFactory = matriculaRepoFactory;
    }

    public async Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var acesso = await _repoFactory().ObterPorId(id, cancellationToken);
        if (acesso == null) return null;
        return (await ParaDtosAsync([acesso], cancellationToken)).First();
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterTodos(cancellationToken);
        return await ParaDtosAsync(acessos, cancellationToken);
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterPorAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var acessos = await _repoFactory().ObterAcessosPorAlunoPeriodo(alunoId, null, null, cancellationToken);
        return await ParaDtosAsync(acessos, cancellationToken);
    }

    public async Task<IEnumerable<AcessoAlunoDto>> ObterDeHojeAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Now);
        var acessos = await _repoFactory().ObterAcessosPorAlunoPeriodo(null, hoje, hoje, cancellationToken);
        return await ParaDtosAsync(acessos, cancellationToken);
    }

    public async Task<AcessoAlunoDto?> ObterAcessoAbertoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var ultimo = await _repoFactory().ObterUltimoAcesso(alunoId, cancellationToken);
        if (ultimo is not { EstaDentro: true }) return null;
        return (await ParaDtosAsync([ultimo], cancellationToken)).First();
    }

    public async Task<AcessoAlunoDto> RegistrarEntradaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var aluno = await _alunoRepoFactory().ObterPorId(alunoId, cancellationToken)
            ?? throw new InvalidOperationException($"Aluno com ID {alunoId} não encontrado.");

        var agora = DateTime.Now;
        var hoje = DateOnly.FromDateTime(agora);
        var dto = new AcessoAlunoDto { Aluno = aluno.ToDto(), Entrada = agora };

        // Já está dentro? Precisa registrar a saída antes.
        var ultimo = await repo.ObterUltimoAcesso(alunoId, cancellationToken);
        if (ultimo is { EstaDentro: true })
            return Negar(dto, "Acesso negado: o aluno já possui uma entrada em aberto. Registre a saída primeiro.");

        // Regra: validar matrícula ativa (início <= hoje <= fim)
        var matriculas = await _matriculaRepoFactory().ObterPorAluno(alunoId, cancellationToken);
        var ativa = matriculas
            .Where(m => m.DataInicio <= hoje && m.DataFim >= hoje)
            .OrderByDescending(m => m.DataFim)
            .FirstOrDefault();

        if (ativa == null)
            return Negar(dto, "Acesso negado: o aluno não possui matrícula ativa.");

        var criar = AcessoAluno.Criar(0, aluno, agora);
        if (criar.IsFailure)
            return Negar(dto, "Acesso negado: fora do horário de funcionamento (06:00 às 22:00).");

        var salvo = await repo.Adicionar(criar.Value!, cancellationToken);

        // Regra: na entrada, mostrar quanto tempo ainda tem de plano
        int dias = ativa.DataFim.DayNumber - hoje.DayNumber;

        dto.Id = salvo.Id;
        dto.Autorizado = true;
        dto.DiasRestantesPlano = dias;
        dto.Mensagem = $"Entrada autorizada. Dias restantes de plano: {dias} dia(s).";
        return dto;
    }

    public async Task<AcessoAlunoDto> RegistrarSaidaAsync(int acessoId, CancellationToken cancellationToken = default)
    {
        var repo = _repoFactory();

        var acesso = await repo.ObterPorId(acessoId, cancellationToken)
            ?? throw new InvalidOperationException($"Acesso com ID {acessoId} não encontrado.");

        var aluno = await _alunoRepoFactory().ObterPorId(acesso.AlunoId, cancellationToken);
        var dto = new AcessoAlunoDto
        {
            Id = acesso.Id,
            Aluno = aluno?.ToDto(),
            Entrada = acesso.DataHora,
            Saida = acesso.DataHoraSaida
        };

        var resultado = acesso.RegistrarSaida(DateTime.Now);
        if (resultado.IsFailure)
            return Negar(dto, "Saída não registrada: este acesso já possui saída.");

        await repo.Atualizar(acesso, cancellationToken);

        dto.Saida = acesso.DataHoraSaida;
        dto.Autorizado = true;
        // Regra: na saída, mostrar o tempo que permaneceu na academia
        dto.Mensagem = $"Saída registrada. Tempo de permanência: {acesso.TempoPermanencia!.Value.ToHoraMinutoSegundo()}.";
        return dto;
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
        => await _repoFactory().Remover(id, cancellationToken);

    private static AcessoAlunoDto Negar(AcessoAlunoDto dto, string mensagem)
    {
        dto.Autorizado = false;
        dto.Mensagem = mensagem;
        return dto;
    }

    private async Task<List<AcessoAlunoDto>> ParaDtosAsync(IEnumerable<AcessoAluno> acessos, CancellationToken cancellationToken)
    {
        var alunoRepo = _alunoRepoFactory();
        var cache = new Dictionary<int, AlunoDto?>();
        var lista = new List<AcessoAlunoDto>();

        foreach (var a in acessos)
        {
            if (!cache.TryGetValue(a.AlunoId, out var alunoDto))
            {
                var aluno = await alunoRepo.ObterPorId(a.AlunoId, cancellationToken);
                alunoDto = aluno?.ToDto();
                cache[a.AlunoId] = alunoDto;
            }

            lista.Add(new AcessoAlunoDto
            {
                Id = a.Id,
                Aluno = alunoDto,
                Entrada = a.DataHora,
                Saida = a.DataHoraSaida
            });
        }
        return lista;
    }
}