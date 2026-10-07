// Jonathan de Souza Pereira

using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    // ============================================================
    // CONFIGURAÇÕES PADRÃO DO MYSQL
    // ============================================================

    private const string ServidorPadraoMySql = "10.0.2.2";

    private const string BancoPadraoMySql = "db_academia_do_ze";

    private const string UsuarioPadraoMySql = "root";

    private const string SenhaPadraoMySql = "abcBolinhas12345";


    // ============================================================
    // CONSTRUTOR
    // ============================================================

    public ConfigPage()
    {
        InitializeComponent();

        CarregarTema();

        CarregarBanco();
    }


    // ============================================================
    // TEMA
    // ============================================================

    private void CarregarTema()
    {
        var temaAtual = Preferences.Get(
            "Tema",
            "system");

        TemaPicker.SelectedItem = temaAtual;
    }


    private async void OnSalvarTemaClicked(
        object sender,
        EventArgs e)
    {
        if (TemaPicker.SelectedItem is not string tema)
        {
            await MostrarAlertaAsync(
                "Atenção",
                "Selecione um tema.");

            return;
        }


        // Salva a preferência
        Preferences.Set(
            "Tema",
            tema);


        // Aplica o tema imediatamente
        Microsoft.Maui.Controls.Application.Current!.UserAppTheme =
            tema switch
            {
                "light" => AppTheme.Light,

                "dark" => AppTheme.Dark,

                _ => AppTheme.Unspecified
            };


        // Envia mensagem para a aplicação
        WeakReferenceMessenger.Default.Send(
            new TemaPreferencesUpdatedMessage(tema));


        await MostrarAlertaAsync(
            "Sucesso",
            "Tema salvo com sucesso!");
    }


    // ============================================================
    // POPUP PERSONALIZADO
    // ============================================================

    private async Task MostrarAlertaAsync(
        string titulo,
        string mensagem)
    {
        await this.ShowPopupAsync(
            new AppAlertPopup(
                titulo,
                mensagem));
    }


    // ============================================================
    // BANCO DE DADOS
    // ============================================================

    private void CarregarBanco()
    {
        DatabaseTypePicker.Items.Clear();


        foreach (var tipo in Enum.GetValues<AppDatabaseType>())
        {
            DatabaseTypePicker.Items.Add(
                tipo.ToString());
        }


        var bancoAtual = Preferences.Get(
            "DatabaseType",
            AppDatabaseType.MySql.ToString());


        DatabaseTypePicker.SelectedItem =
            bancoAtual;


        AtualizarInterfacePorTipoBanco();
    }


    private void OnDatabaseTypeChanged(
        object sender,
        EventArgs e)
    {
        AtualizarInterfacePorTipoBanco();
    }


    private void AtualizarInterfacePorTipoBanco()
    {
        if (DatabaseTypePicker.SelectedItem is not string tipo)
        {
            return;
        }


        // ========================================================
        // SQLITE
        // ========================================================

        if (tipo == AppDatabaseType.Sqlite.ToString())
        {
            SqliteInfoCard.IsVisible = true;

            SqliteContainer.IsVisible = true;

            ServidorBancoGrid.IsVisible = false;

            CredenciaisGrid.IsVisible = false;

            ComplementoLabel.IsVisible = false;

            ComplementoEntry.IsVisible = false;


            SqliteCaminhoEntry.Text =
                Preferences.Get(
                    "Sqlite_Caminho",
                    "db_academia_do_ze.db");


            return;
        }


        // ========================================================
        // SQL SERVER / MYSQL
        // ========================================================

        SqliteInfoCard.IsVisible = false;

        SqliteContainer.IsVisible = false;

        ServidorBancoGrid.IsVisible = true;

        CredenciaisGrid.IsVisible = true;

        ComplementoLabel.IsVisible = true;

        ComplementoEntry.IsVisible = true;


        // ========================================================
        // SQL SERVER
        // ========================================================

        if (tipo == AppDatabaseType.SqlServer.ToString())
        {
            ServidorEntry.Placeholder =
                "Ex: localhost,1433";

            BancoEntry.Placeholder =
                "Nome do banco";

            UsuarioEntry.Placeholder =
                "Usuário";

            SenhaEntry.Placeholder =
                "Senha";

            ComplementoLabel.Text =
                "Complemento";


            ServidorEntry.Text =
                Preferences.Get(
                    "SqlServer_Servidor",
                    "localhost");


            BancoEntry.Text =
                Preferences.Get(
                    "SqlServer_Banco",
                    "db_academia_do_ze");


            UsuarioEntry.Text =
                Preferences.Get(
                    "SqlServer_Usuario",
                    "sa");


            SenhaEntry.Text =
                Preferences.Get(
                    "SqlServer_Senha",
                    "");


            ComplementoEntry.Text =
                Preferences.Get(
                    "SqlServer_Complemento",
                    "TrustServerCertificate=True;");


            return;
        }


        // ========================================================
        // MYSQL
        // ========================================================

        if (tipo == AppDatabaseType.MySql.ToString())
        {
            ServidorEntry.Placeholder =
                "Ex: 10.0.2.2";

            BancoEntry.Placeholder =
                "Nome do banco";

            UsuarioEntry.Placeholder =
                "Usuário";

            SenhaEntry.Placeholder =
                "Senha";

            ComplementoLabel.Text =
                "Complemento";


            ServidorEntry.Text =
                Preferences.Get(
                    "MySql_Servidor",
                    ServidorPadraoMySql);


            BancoEntry.Text =
                Preferences.Get(
                    "MySql_Banco",
                    BancoPadraoMySql);


            UsuarioEntry.Text =
                Preferences.Get(
                    "MySql_Usuario",
                    UsuarioPadraoMySql);


            SenhaEntry.Text =
                Preferences.Get(
                    "MySql_Senha",
                    SenhaPadraoMySql);


            ComplementoEntry.Text =
                Preferences.Get(
                    "MySql_Complemento",
                    "Port=3306;Connection Timeout=5;Default Command Timeout=30;");


            return;
        }
    }


    // ============================================================
    // SALVAR BANCO
    // ============================================================

    private async void OnSalvarBdClicked(
        object sender,
        EventArgs e)
    {
        if (DatabaseTypePicker.SelectedItem is not string tipo)
        {
            await MostrarAlertaAsync(
                "Atenção",
                "Selecione o tipo de banco de dados.");

            return;
        }


        // ========================================================
        // SQLITE
        // ========================================================

        if (tipo == AppDatabaseType.Sqlite.ToString())
        {
            var caminho =
                SqliteCaminhoEntry.Text?.Trim();


            if (string.IsNullOrWhiteSpace(caminho))
            {
                await MostrarAlertaAsync(
                    "Atenção",
                    "Informe o caminho do banco SQLite.");

                return;
            }


            Preferences.Set(
                "DatabaseType",
                tipo);

            Preferences.Set(
                "Sqlite_Caminho",
                caminho);


            WeakReferenceMessenger.Default.Send(
                new BancoPreferencesUpdatedMessage(tipo));


            await MostrarAlertaAsync(
                "Sucesso",
                "Configuração do SQLite salva com sucesso!");


            await Shell.Current.GoToAsync(
                "//dashboard");

            return;
        }


        // ========================================================
        // MYSQL / SQL SERVER
        // ========================================================

        var servidor =
            ServidorEntry.Text?.Trim();

        var banco =
            BancoEntry.Text?.Trim();

        var usuario =
            UsuarioEntry.Text?.Trim();

        var senha =
            SenhaEntry.Text ?? "";

        var complemento =
            ComplementoEntry.Text?.Trim() ?? "";


        if (string.IsNullOrWhiteSpace(servidor))
        {
            await MostrarAlertaAsync(
                "Atenção",
                "Informe o servidor.");

            return;
        }


        if (string.IsNullOrWhiteSpace(banco))
        {
            await MostrarAlertaAsync(
                "Atenção",
                "Informe o banco de dados.");

            return;
        }


        if (string.IsNullOrWhiteSpace(usuario))
        {
            await MostrarAlertaAsync(
                "Atenção",
                "Informe o usuário.");

            return;
        }


        Preferences.Set(
            "DatabaseType",
            tipo);


        // ========================================================
        // MYSQL
        // ========================================================

        if (tipo == AppDatabaseType.MySql.ToString())
        {
            Preferences.Set(
                "MySql_Servidor",
                servidor);

            Preferences.Set(
                "MySql_Banco",
                banco);

            Preferences.Set(
                "MySql_Usuario",
                usuario);

            Preferences.Set(
                "MySql_Senha",
                senha);

            Preferences.Set(
                "MySql_Complemento",
                complemento);
        }


        // ========================================================
        // SQL SERVER
        // ========================================================

        else if (tipo == AppDatabaseType.SqlServer.ToString())
        {
            Preferences.Set(
                "SqlServer_Servidor",
                servidor);

            Preferences.Set(
                "SqlServer_Banco",
                banco);

            Preferences.Set(
                "SqlServer_Usuario",
                usuario);

            Preferences.Set(
                "SqlServer_Senha",
                senha);

            Preferences.Set(
                "SqlServer_Complemento",
                complemento);
        }


        WeakReferenceMessenger.Default.Send(
            new BancoPreferencesUpdatedMessage(tipo));


        await MostrarAlertaAsync(
            "Sucesso",
            "Configuração do banco salva com sucesso!");


        await Shell.Current.GoToAsync(
            "//dashboard");
    }


    // ============================================================
    // CANCELAR
    // ============================================================

    private async void OnCancelarClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//dashboard");
    }


    // ============================================================
    // CICLO DE VIDA
    // ============================================================

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        WeakReferenceMessenger.Default.UnregisterAll(
            this);
    }
}