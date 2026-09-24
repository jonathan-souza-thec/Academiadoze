// Jonathan de Souza Pereira
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Tests.Entities;

public class ColaboradorTests
{
    private static Logradouro GetValidLogradouro() => Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    private static Arquivo GetValidArquivo() => Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!;

    [Theory(DisplayName = "Colaborador: criação bem-sucedida com nomes válidos (trim aplicado)")]
    [InlineData(" Carlos Souza ")]
    [InlineData("Ana")]
    public void Deve_Criar_Com_Sucesso_Quando_NomeValido(string nome)
    {
        var result = Colaborador.Criar(
            1,
            nome,
            "529.982.247-25",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678",
            "user@example.com",
            GetValidLogradouro(),
            "123",
            "",
            "Abcdef",
            GetValidArquivo(),
            DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Atendente,
            ColaboradorVinculo.CLT
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(nome.Trim(), result.Value!.Nome);
    }

    [Theory(DisplayName = "Colaborador: nome vazio -> NOME_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_NomeVazio(string nome)
    {
        var result = Colaborador.Criar(
            1, nome, "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef",
            GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Atendente, ColaboradorVinculo.CLT
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Theory(DisplayName = "Colaborador: data nascimento -> obrigatoriedade e idade mínima")]
    [InlineData("default", "DATA_NASCIMENTO_OBRIGATORIO")]
    [InlineData("under12", "DATA_NASCIMENTO_MINIMA_INVALIDA")]
    public void Deve_Falhar_Criacao_Quando_DataNascimentoInvalida(string scenario, string expectedMessage)
    {
        var dataNascimento = scenario == "default"
            ? default(DateOnly)
            : DateOnly.FromDateTime(DateTime.Today.AddYears(-10));
        var result = Colaborador.Criar(
            1, "Carlos Souza", "529.982.247-25", dataNascimento,
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef",
            GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Atendente, ColaboradorVinculo.CLT
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == expectedMessage);
    }

    [Theory(DisplayName = "Colaborador: data admissão -> obrigatoriedade e não pode ser futura")]
    [InlineData("default", "DATA_ADMISSAO_OBRIGATORIO")]
    [InlineData("futura", "DATA_ADMISSAO_MAIOR_QUE_ATUAL")]
    public void Deve_Falhar_Criacao_Quando_DataAdmissaoInvalida(string scenario, string expectedMessage)
    {
        var dataAdmissao = scenario == "default"
            ? default(DateOnly)
            : DateOnly.FromDateTime(DateTime.Today.AddDays(10));
        var result = Colaborador.Criar(
            1, "Carlos Souza", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef",
            GetValidArquivo(), dataAdmissao, ColaboradorTipo.Atendente, ColaboradorVinculo.CLT
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == expectedMessage);
    }

    [Theory(DisplayName = "Colaborador: data admissão válida (hoje ou passado) -> sucesso")]
    [InlineData(0)]
    [InlineData(-365)]
    public void Deve_Criar_Com_Sucesso_Quando_DataAdmissaoValida(int diasOffset)
    {
        var dataAdmissao = DateOnly.FromDateTime(DateTime.Today.AddDays(diasOffset));
        var result = Colaborador.Criar(
            1, "Carlos Souza", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef",
            GetValidArquivo(), dataAdmissao, ColaboradorTipo.Atendente, ColaboradorVinculo.CLT
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(dataAdmissao, result.Value!.DataAdmissao);
    }

    [Theory(DisplayName = "Colaborador: tipo inválido -> TIPO_COLABORADOR_INVALIDO")]
    [InlineData(99)]
    [InlineData(-1)]
    public void Deve_Falhar_Criacao_Quando_TipoInvalido(int tipoValue)
    {
        var tipo = (ColaboradorTipo)tipoValue;
        var result = Colaborador.Criar(
            1, "Carlos Souza", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef",
            GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), tipo, ColaboradorVinculo.CLT
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TIPO_COLABORADOR_INVALIDO");
    }

    [Theory(DisplayName = "Colaborador: vínculo inválido -> VINCULO_COLABORADOR_INVALIDO")]
    [InlineData(99)]
    [InlineData(-1)]
    public void Deve_Falhar_Criacao_Quando_VinculoInvalido(int vinculoValue)
    {
        var vinculo = (ColaboradorVinculo)vinculoValue;
        var result = Colaborador.Criar(
            1, "Carlos Souza", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef",
            GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, vinculo
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "VINCULO_COLABORADOR_INVALIDO");
    }

    [Fact(DisplayName = "Colaborador: administrador com vínculo diferente de CLT -> ADMINISTRADOR_CLT_INVALIDO")]
    public void Deve_Falhar_Criacao_Quando_AdministradorNaoEhCLT()
    {
        var result = Colaborador.Criar(
            1, "Carlos Souza", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef",
            GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            ColaboradorTipo.Administrador, ColaboradorVinculo.Estagio
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ADMINISTRADOR_CLT_INVALIDO");
    }

    [Theory(DisplayName = "Colaborador: criação bem-sucedida com combinações válidas de tipo e vínculo")]
    [InlineData(ColaboradorTipo.Administrador, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Atendente, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Atendente, ColaboradorVinculo.Estagio)]
    [InlineData(ColaboradorTipo.Instrutor, ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorTipo.Instrutor, ColaboradorVinculo.Estagio)]
    public void Deve_Criar_Com_Sucesso_Quando_TipoEVinculoValidos(ColaboradorTipo tipo, ColaboradorVinculo vinculo)
    {
        var result = Colaborador.Criar(
            1, "Carlos Souza", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef",
            GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), tipo, vinculo
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(tipo, result.Value!.Tipo);
        Assert.Equal(vinculo, result.Value.Vinculo);
    }

    [Fact(DisplayName = "Colaborador: múltiplos campos inválidos -> várias notificações simultâneas")]
    public void Deve_Falhar_Criacao_Com_MultiplasNotificacoes_Quando_VariosCamposInvalidos()
    {
        var result = Colaborador.Criar(
            1, "", "123", default, "123", "invalido",
            GetValidLogradouro(), "123", "", "abc",
            GetValidArquivo(), default, (ColaboradorTipo)99, (ColaboradorVinculo)99
        );
        Assert.True(result.IsFailure);
        Assert.True(result.Notifications.Count > 1);
    }
}