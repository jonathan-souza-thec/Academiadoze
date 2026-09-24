// Jonathan de Souza Pereira
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    [Theory(DisplayName = "Cep: dígitos inválidos -> CEP_DIGITOS")]
    [InlineData
    ("123")]
    [InlineData
    ("12-345")]
    public void Deve_Falhar_Criacao_Quando_CepDigitosInvalidos
    (string input)

    {
        var result = Cep
        .Criar
        (input);

        Assert
        .True
        (result.IsFailure);

        Assert
        .NotEmpty
        (result.Notifications);

    }
    [Theory(DisplayName = "Cep: formatos válidos (com e sem hífen)")]
    [InlineData
    ("12345-678")]
    [InlineData
    ("12345678")]
    public void Deve_Criar_Cep_Quando_Valido
    (string input)

    {
        var result = Cep
        .Criar
        (input);

        Assert
        .True
        (result.IsSuccess);

        Assert
        .Equal
        ("12345678", result.Value!.Valor);

    }
    [Theory(DisplayName = "Cep: obrigatório -> CEP_OBRIGATORIO")]
    [InlineData
    (null)]
    [InlineData
    ("")]
    public void Deve_Falhar_Criacao_Quando_CepNuloOuVazio
    (string? input)

    {
        var result = Cep
        .Criar
        (input!);

        Assert
        .True
        (result.IsFailure);

        Assert
        .Contains
        (result.Notifications,
        n =>
        n.Mensagem == "CEP_OBRIGATORIO");

    }
    [Theory(DisplayName = "Endereco: criação válida com número e complemento")]
    [InlineData
    ("10", "Bloco A")]
    [InlineData
    ("1", "")]
    public void Deve_Criar_Endereco_Quando_Valido

    (string numero, string complemento)

    {
        var logradouro = Logradouro

        .Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

        var result = Endereco
        .Criar
        (logradouro, numero, complemento);

        Assert
        .True
        (result.IsSuccess);

        Assert
        .Equal
        (logradouro.Id, result.Value!.LogradouroId);

        Assert
        .Equal
        (numero, result.Value.Numero);

        Assert
        .Equal
        (complemento, result.Value.Complemento);

    }
    [Theory(DisplayName = "Endereco: valida obrigatoriedade do logradouro e número")]
    [InlineData
    ("invalid", "1", "LOGRADOURO_OBRIGATORIO")]
    [InlineData
    ("valid", "", "NUMERO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_EnderecoInvalido

    (string logradouroCase, string numero, string expected)

    {
        Logradouro? logradouro = null;
        if (logradouroCase == "valid")
            logradouro = Logradouro

            .Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

        var result = Endereco
        .Criar
        (logradouro!, numero, "");

        Assert
        .True
        (result.IsFailure);

        Assert
        .Contains
        (result.Notifications,
        n =>
        n.Mensagem == expected);

    }

    [Theory(DisplayName = "Cpf: nulo/vazio/espaços -> CPF_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_CpfNuloOuVazio(string? input)
    {
        var result = Cpf.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Single(result.Notifications);
        Assert.Equal("CPF_OBRIGATORIO", result.Notifications.First().Mensagem);
    }
    [Theory(DisplayName = "Cpf: formatos válidos (com e sem pontuação)")]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Deve_Criar_Cpf_Quando_ValorValido(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal("52998224725", result.Value!.Valor);
    }

    [Fact]
    public void Deve_Falhar_Criacao_Quando_CpfSemDigitos()
    {
        string input = "123456789"; // Hardcode the value
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_DIGITOS");
    }



    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("119123456")]
    [InlineData("119123456789")]
    public void Deve_Falhar_Criacao_Quando_TelefoneDigitosInvalidos(string input)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
    }

    [Theory(DisplayName = "Telefone: formatos válidos (com e sem formatação)")]
    [InlineData("(11) 91234-5678")]
    [InlineData("11912345678")]




    public void Deve_Passar_Criacao_Quando_TelefoneValido(string input)
    {
        var result = Telefone.Criar(input);

        Assert.True(result.IsSuccess);
    }



    [Fact(DisplayName = "Telefone: criação válida -> formatação padronizada")]
    public void Deve_Criar_Telefone_Quando_Valido()
    {
        string input = "(11) 91234-5678";
        var result = Telefone.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal("11912345678", result.Value!.Valor);
    }




    [Theory(DisplayName = "Telefone: obrigatório -> TELEFONE_OBRIGATORIO")]
    [InlineData(null)]
    [InlineData("")]
    public void Deve_Falhar_Criacao_Quando_TelefoneNuloOuVazio(string? input)
    {
        var result = Telefone.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TELEFONE_OBRIGATORIO");
    }



    [Theory(DisplayName = "Senha: valida requisito de uppercase")]
    [InlineData("abcdef", false)]
    [InlineData("Abcdef", true)]
    public void Deve_Validar_RequisitoUppercase_Senha(string senha, bool isSuccess)
    {
        var result = Senha.Criar(senha);
        Assert.Equal(isSuccess, result.IsSuccess);
    }



    [Fact(DisplayName = "Senha: obrigatório -> SENHA_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_SenhaNulaOuVazia()
    {
        // Test null
        var resultNull = Senha.Criar(null!);
        Assert.True(resultNull.IsFailure);
        Assert.Contains(resultNull.Notifications, n => n.Mensagem == "SENHA_OBRIGATORIO");

        // Test empty
        var resultEmpty = Senha.Criar("");
        Assert.True(resultEmpty.IsFailure);
        Assert.Contains(resultEmpty.Notifications, n => n.Mensagem == "SENHA_OBRIGATORIO");
    }



    [Theory(DisplayName = "Email: nulo/vazio/espaços -> EMAIL_FORMATO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_EmailVazioOuNulo(string? input)
    {
        var result = Email.Criar(input!);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }


    [Theory(DisplayName = "Email: formato inválido -> EMAIL_FORMATO")]
    [InlineData("abc")]
    [InlineData("abc@")]
    [InlineData("abc@dominio")]
    [InlineData("abc@.com")]
    [InlineData("abc@dominio.")]
    public void Deve_Falhar_Criacao_Quando_EmailFormatoInvalido(string input)
    {
        var result = Email.Criar(input);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }



    [Theory(DisplayName = "Email: formatos válidos (com espaços a serem removidos)")]
    [InlineData("user@example.com", "user@example.com")]
    [InlineData(" user@example.com ", "user@example.com")]
    public void Deve_Criar_Email_Quando_Valido(string input, string expected)
    {
        var result = Email.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
    }



    [Fact(DisplayName = "Arquivo: conteúdo nulo -> ARQUIVO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_ConteudoNulo()
    {
        var result = Arquivo.Criar(null!);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }


    [Fact(DisplayName = "Arquivo: tamanho acima de 15MB -> ARQUIVO_TIPO_TAMANHO")]
    public void Deve_Falhar_Criacao_Quando_TamanhoExcedeLimite()
    {
        var conteudoGrande = new byte[15 * 1024 * 1024 + 1];
        var result = Arquivo.Criar(conteudoGrande);
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_TIPO_TAMANHO");
    }




    [Fact(DisplayName = "Arquivo: conteúdo válido -> criação bem-sucedida")]
    public void Deve_Criar_Arquivo_Quando_ConteudoValido()
    {
        var conteudo = new byte[] { 1, 2, 3, 4, 5 };
        var result = Arquivo.Criar(conteudo);
        Assert.True(result.IsSuccess);
        Assert.Equal(conteudo, result.Value!.Conteudo);
    }
}