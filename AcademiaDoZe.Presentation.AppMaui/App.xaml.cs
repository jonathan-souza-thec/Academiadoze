// Jonathan de Souza Pereira

using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui;

// Application conflita com o nome da nossa camada Application.
// Por isso utilizamos o namespace completo do MAUI.
public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();


        // ========================================================
        // APLICA O TEMA SALVO
        // ========================================================

        AplicarTema();


        // ========================================================
        // ESCUTA ALTERAÇÕES DO TEMA
        // ========================================================

        WeakReferenceMessenger.Default.Register<TemaPreferencesUpdatedMessage>(
            this,
            (r, m) =>
            {
                AplicarTema();
            });
    }


    // ============================================================
    // CRIA A JANELA PRINCIPAL
    // ============================================================

    protected override Window CreateWindow(
        IActivationState? activationState)
    {
        return new Window(
            new AppShell());
    }


    // ============================================================
    // APLICA O TEMA
    // ============================================================

    private void AplicarTema()
    {
        var tema =
            Preferences.Get(
                "Tema",
                "system");


        UserAppTheme =
            tema switch
            {
                "light" => AppTheme.Light,

                "dark" => AppTheme.Dark,

                _ => AppTheme.Unspecified
            };
    }
}