// Jonathan de Souza Pereira

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("logradouro", typeof(Views.LogradouroPage));
    }
}