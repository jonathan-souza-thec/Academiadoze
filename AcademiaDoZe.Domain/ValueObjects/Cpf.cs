// Jonathan de Souza Pereira
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get; }

    private Cpf(string valor)
    {
        Valor = valor;
    }

    public static Result<Cpf> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Cpf>.Failure("Cpf", "CPF_OBRIGATORIO");

        // normaliza: remove pontos, traços e espaços
        var textoLimpo = NormalizadoService.LimparEDigitos(valor);

        if (textoLimpo.Length != 11)
            return Result<Cpf>.Failure("Cpf", "CPF_DIGITOS");

        // valida dígitos verificadores
        if (!ValidarDigitos(textoLimpo))
            return Result<Cpf>.Failure("Cpf", "CPF_INVALIDO");

        return Result<Cpf>.Success(new Cpf(textoLimpo));
    }

    private static bool ValidarDigitos(string cpf)
    {
        // rejeita sequências repetidas (ex.: 000.000.000-00)
        if (cpf.Distinct().Count() == 1) return false;

        int[] multiplicadores1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicadores2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        var soma = cpf.Take(9).Select((c, i) => (c - '0') * multiplicadores1[i]).Sum();
        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;
        if (cpf[9] - '0' != digito1) return false;

        soma = cpf.Take(10).Select((c, i) => (c - '0') * multiplicadores2[i]).Sum();
        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;
        return cpf[10] - '0' == digito2;
    }

    public override string ToString() => Valor;
}
