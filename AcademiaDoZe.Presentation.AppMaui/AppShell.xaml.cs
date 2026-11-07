// Jonathan de Souza Pereira

using AcademiaDoZe.Presentation.AppMaui.Views;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rotas de páginas de detalhe/cadastro (cada rota deve ser registrada apenas uma vez)
        Routing.RegisterRoute("logradouro", typeof(LogradouroPage));
        Routing.RegisterRoute("colaborador", typeof(ColaboradorPage));
        Routing.RegisterRoute("aluno", typeof(AlunoPage));
        Routing.RegisterRoute("matricula", typeof(MatriculaPage));
    }
}