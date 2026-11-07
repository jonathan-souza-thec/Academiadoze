// Jonathan de Souza Pereira
namespace AcademiaDoZe.Application.Exceptions;

// Lançada quando uma regra de negócio impede a entrada/saída (ex: sem matrícula ativa, limite de horas).
// A camada de apresentação usa essa exceção para exibir "Acesso Negado" em vez de um erro genérico.
public class AcessoNegadoException : InvalidOperationException
{
    public AcessoNegadoException(string message) : base(message)
    {
    }
}