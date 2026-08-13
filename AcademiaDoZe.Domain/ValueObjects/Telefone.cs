// Jonathan de Souza Pereira
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Telefone
{
    public string Valor { get; }

    private Telefone(string valor)
    {
        Valor = valor;
    }

    public static Result<Telefone> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Telefone>.Failure("Telefone", "TELEFONE_OBRIGATORIO");

        // normaliza: mantém somente dígitos (aceita (49) 99999-9999 e 49999999999)
        var textoLimpo = NormalizadoService.LimparEDigitos(valor);

        // celular = 11 dígitos (DDD + 9 + número); fixo = 10 dígitos
        if (textoLimpo.Length != 11)
            return Result<Telefone>.Failure("Telefone", "TELEFONE_DIGITOS");

        return Result<Telefone>.Success(new Telefone(textoLimpo));
    }

    public override string ToString() => Valor;
}
