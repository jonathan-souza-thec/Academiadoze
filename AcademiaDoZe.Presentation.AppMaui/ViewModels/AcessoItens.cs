// Jonathan de Souza Pereira
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public record PessoaItem(int Id, string Nome, string Cpf)
{
    public string Descricao => $"CPF: {Cpf}";
}

public record RegistroHojeItem(string Nome, string Descricao, DateTime Hora)
{
    public string HoraTexto => Hora.ToString("HH:mm:ss");
}