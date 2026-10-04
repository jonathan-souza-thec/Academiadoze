// Jonathan de Souza Pereira

using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    // Servidor padrão para MySQL no Android Emulator.
    // 10.0.2.2 aponta para o computador Windows.
    private const string ServidorPadraoMySql = "10.0.2.2";

    // Banco padrão
    private const string BancoPadraoMySql = "db_academia_do_ze";

    // Usuário padrão
    private const string UsuarioPadraoMySql = "root";

    // Senha padrão
    private const string SenhaPadraoMySql = "abcBolinhas12345";

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
            await DisplayAlertAsync(
                "Atenção",
                "Selecione um tema.",
                "OK");

            return;
        }

        Preferences.Set(
            "Tema",
            tema);

        // A classe da mensagem exige um string.
        WeakReferenceMessenger.Default.Send(
            new TemaPreferencesUpdatedMessage(tema));

        await DisplayAlertAsync(
            "Sucesso",
            "Tema salvo com sucesso!",
            "OK");
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

        DatabaseTypePicker.SelectedItem = bancoAtual;

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

            SqliteCaminhoEntry.Text = Preferences.Get(
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

            ServidorEntry.Text = Preferences.Get(
                "SqlServer_Servidor",
                "localhost");

            BancoEntry.Text = Preferences.Get(
                "SqlServer_Banco",
                "db_academia_do_ze");

            UsuarioEntry.Text = Preferences.Get(
                "SqlServer_Usuario",
                "sa");

            SenhaEntry.Text = Preferences.Get(
                "SqlServer_Senha",
                "");

            ComplementoEntry.Text = Preferences.Get(
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

            ServidorEntry.Text = Preferences.Get(
                "MySql_Servidor",
                ServidorPadraoMySql);

            BancoEntry.Text = Preferences.Get(
                "MySql_Banco",
                BancoPadraoMySql);

            UsuarioEntry.Text = Preferences.Get(
                "MySql_Usuario",
                UsuarioPadraoMySql);

            SenhaEntry.Text = Preferences.Get(
                "MySql_Senha",
                SenhaPadraoMySql);

            ComplementoEntry.Text = Preferences.Get(
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
            await DisplayAlertAsync(
                "Atenção",
                "Selecione o tipo de banco de dados.",
                "OK");

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
                await DisplayAlertAsync(
                    "Atenção",
                    "Informe o caminho do banco SQLite.",
                    "OK");

                return;
            }

            Preferences.Set(
                "DatabaseType",
                tipo);

            Preferences.Set(
                "Sqlite_Caminho",
                caminho);

            // A mensagem exige um string.
            WeakReferenceMessenger.Default.Send(
                new BancoPreferencesUpdatedMessage(tipo));

            await DisplayAlertAsync(
                "Sucesso",
                "Configuração do SQLite salva com sucesso!",
                "OK");

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
            await DisplayAlertAsync(
                "Atenção",
                "Informe o servidor.",
                "OK");

            return;
        }

        if (string.IsNullOrWhiteSpace(banco))
        {
            await DisplayAlertAsync(
                "Atenção",
                "Informe o banco de dados.",
                "OK");

            return;
        }

        if (string.IsNullOrWhiteSpace(usuario))
        {
            await DisplayAlertAsync(
                "Atenção",
                "Informe o usuário.",
                "OK");

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

        // A mensagem exige um string.
        WeakReferenceMessenger.Default.Send(
            new BancoPreferencesUpdatedMessage(tipo));

        await DisplayAlertAsync(
            "Sucesso",
            "Configuração do banco salva com sucesso!",
            "OK");

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