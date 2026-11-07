// Jonathan de Souza Pereira

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;

namespace AcademiaDoZe.Presentation.AppMaui.Services;

// Cria dados de demonstração para a gravação do vídeo da Avaliação 03.
public class DemoDataSeeder
{
    private readonly ILogradouroService _logradouroService;
    private readonly IAlunoService _alunoService;
    private readonly IMatriculaService _matriculaService;
    private readonly IColaboradorService _colaboradorService;

    public DemoDataSeeder(
        ILogradouroService logradouroService,
        IAlunoService alunoService,
        IMatriculaService matriculaService,
        IColaboradorService colaboradorService)
    {
        _logradouroService = logradouroService;
        _alunoService = alunoService;
        _matriculaService = matriculaService;
        _colaboradorService = colaboradorService;
    }

    private static ArquivoDto FotoPadrao() => new() { Conteudo = [1, 2, 3] };

    public async Task<string> CriarAsync(CancellationToken ct = default)
    {
        var criados = new List<string>();

        var logradouro = await _logradouroService.ObterPorCepAsync("89520000", ct)
            ?? await _logradouroService.AdicionarAsync(new LogradouroDto
            {
                Cep = "89520000",
                Nome = "Rua Demonstração",
                Bairro = "Centro",
                Cidade = "Lages",
                Estado = "SC",
                Pais = "Brasil"
            }, ct);

        // 1) Aluno COM matrícula ativa (nome completo)
        var matriculado = await _alunoService.ObterPorCpfAsync("529.982.247-25", ct);
        if (matriculado == null)
        {
            matriculado = await _alunoService.AdicionarAsync(NovoAluno(
                "Jonathan de Souza Pereira", "529.982.247-25", "jonathan.demo@academiadoze.com", logradouro), ct);

            await _matriculaService.AdicionarAsync(new MatriculaDto
            {
                AlunoMatricula = matriculado,
                Plano = AppMatriculaPlano.Semestral,
                DataInicio = DateOnly.FromDateTime(DateTime.Today),
                Objetivo = "Hipertrofia",
                RestricoesMedicas = AppMatriculaRestricoes.None
            }, ct);
            criados.Add("Aluno matriculado: Jonathan de Souza Pereira");
        }

        // 2) Aluno SEM matrícula
        if (await _alunoService.ObterPorCpfAsync("111.444.777-35", ct) == null)
        {
            await _alunoService.AdicionarAsync(NovoAluno(
                "Maria Aparecida da Silva", "111.444.777-35", "maria.demo@academiadoze.com", logradouro), ct);
            criados.Add("Aluno sem matrícula: Maria Aparecida da Silva");
        }

        // 3) Colaborador CLT (nome completo)
        if (await _colaboradorService.ObterPorCpfAsync("390.533.447-05", ct) == null)
        {
            await _colaboradorService.AdicionarAsync(NovoColaborador(
                "Jonathan de Souza Pereira", "390.533.447-05", "colab.demo@academiadoze.com",
                logradouro, AppColaboradorVinculo.CLT), ct);
            criados.Add("Colaborador CLT: Jonathan de Souza Pereira");
        }

        // 4) Colaborador estagiário (limite de 6h)
        if (await _colaboradorService.ObterPorCpfAsync("123.456.789-09", ct) == null)
        {
            await _colaboradorService.AdicionarAsync(NovoColaborador(
                "Carlos Eduardo Pereira", "123.456.789-09", "estagio.demo@academiadoze.com",
                logradouro, AppColaboradorVinculo.Estagio), ct);
            criados.Add("Colaborador estágio: Carlos Eduardo Pereira");
        }

        return criados.Count == 0
            ? "Os dados de demonstração já existem."
            : "Criados:\n" + string.Join("\n", criados);
    }

    private static AlunoDto NovoAluno(string nome, string cpf, string email, LogradouroDto logradouro) => new()
    {
        Nome = nome,
        Cpf = cpf,
        DataNascimento = new DateOnly(1995, 5, 10),
        Telefone = "(49) 91234-5678",
        Email = email,
        Endereco = logradouro,
        Numero = "100",
        Complemento = string.Empty,
        Senha = "Senha123",
        Foto = FotoPadrao()
    };

    private static ColaboradorDto NovoColaborador(
        string nome, string cpf, string email, LogradouroDto logradouro, AppColaboradorVinculo vinculo) => new()
        {
            Nome = nome,
            Cpf = cpf,
            DataNascimento = new DateOnly(1990, 3, 15),
            Telefone = "(49) 98888-7777",
            Email = email,
            Endereco = logradouro,
            Numero = "200",
            Complemento = string.Empty,
            Senha = "Senha123",
            Foto = FotoPadrao(),
            DataAdmissao = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            Tipo = AppColaboradorTipo.Atendente,
            Vinculo = vinculo
        };
}