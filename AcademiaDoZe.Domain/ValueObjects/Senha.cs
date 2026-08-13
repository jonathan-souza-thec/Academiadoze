// Jonathan de Souza Pereira
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Senha
{
    public string Valor { get; }

    private Senha(string valor)
    {
        Valor = valor;
    }

    public static Result<Senha> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Senha>.Failure("Senha", "SENHA_OBRIGATORIA");

        // remove espaços antes e depois, mas preserva espaços internos (senhas podem tê-los)
        var textoLimpo = valor.Trim();

        if (textoLimpo.Length < 8)
            return Result<Senha>.Failure("Senha", "SENHA_MINIMO_8_CARACTERES");

        if (textoLimpo.Length > 100)
            return Result<Senha>.Failure("Senha", "SENHA_MAXIMO_100_CARACTERES");

        return Result<Senha>.Success(new Senha(textoLimpo));
    }

    public override string ToString() => Valor;
}
