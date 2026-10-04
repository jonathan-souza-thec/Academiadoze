using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    // =========================================================
    // CONFIGURAÇÕES PADRÃO
    // =========================================================

    // No Android Emulator, 10.0.2.2 representa o computador Windows.
    private const string ServidorPadraoMySql = "10.0.2.2";

    // Servidor padrão do SQL Server.
    private const string ServidorPadraoSqlServer = "172.24.32.1";

    // Senha padrão usada no projeto.
    private const string SenhaPadrao = "abcBolinhas12345";

    public ConfigPage()
    {
        InitializeComponent();

        CarregarTema();
        CarregarBanco();
    }

    // =========================================================
    // TEMA
    // =========================================================

    private void CarregarTema()
    {
        // Carrega o tema salvo nas Preferences.
        TemaPicker.SelectedIndex = Preferences.Get(
            "Tema",
            "system") switch
        {
            "light" => 0,
            "dark" => 1,
            _ => 2
        };
    }

    private async void OnSalvarTemaClicked(
        object? sender,
        EventArgs e)
    {
        string selectedTheme = TemaPicker.SelectedIndex switch
        {
            0 => "light",
            1 => "dark",
            _ => "system"
        };

        Preferences.Set("Tema", selectedTheme);

        // Envia mensagem para atualizar o tema.
        WeakReferenceMessenger.Default.Send(
            new TemaPreferencesUpdatedMessage("TemaAlterado"));

        await DisplayAlertAsync(
            "Sucesso",
            "Dados salvos com sucesso!",
            "OK");

        await Shell.Current.GoToAsync("//dashboard");
    }

    // =========================================================
    // BANCO DE DADOS
    // =========================================================

    private void CarregarBanco()
    {
        DatabaseTypePicker.Items.Clear();

        foreach (var tipo in Enum.GetValues<AppDatabaseType>())
        {
            DatabaseTypePicker.Items.Add(tipo.ToString());
        }

        var bancoAtual = Preferences.Get(
            "DatabaseType",
            AppDatabaseType.MySql.ToString());

        DatabaseTypePicker.SelectedItem = bancoAtual;

        AtualizarInterfacePorTipoBanco();
    }

    private void OnDatabaseTypeChanged(
        object? sender,
        EventArgs? e)
    {
        AtualizarInterfacePorTipoBanco();
    }

    private void AtualizarInterfacePorTipoBanco()
    {
        if (DatabaseTypePicker.SelectedItem is not string selectedTypeStr ||
            !Enum.TryParse<AppDatabaseType>(
                selectedTypeStr,
                out var selectedType))
        {
            return;
        }

        switch (selectedType)
        {
            // =================================================
            // SQLITE
            // =================================================

            case AppDatabaseType.Sqlite:

                SqliteInfoCard.IsVisible = true;
                SqliteContainer.IsVisible = true;

                ServidorBancoGrid.IsVisible = false;
                CredenciaisGrid.IsVisible = false;

                ComplementoLabel.Text =
                    "Complemento (ex: Default Timeout=5;)";

                ComplementoEntry.Placeholder =
                    "Default Timeout=5;";

                var defaultSqlitePath =
                    DeviceInfo.Platform == DevicePlatform.WinUI
                        ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
                        : Path.Combine(
                            FileSystem.AppDataDirectory,
                            "db_academia_do_ze.db");

                SqliteCaminhoEntry.Text =
                    Preferences.Get(
                        "Sqlite_Caminho",
                        defaultSqlitePath);

                ComplementoEntry.Text =
                    Preferences.Get(
                        "Sqlite_Complemento",
                        "Default Timeout=5;");

                break;

            // =================================================
            // SQL SERVER
            // =================================================

            case AppDatabaseType.SqlServer:

                SqliteInfoCard.IsVisible = false;
                SqliteContainer.IsVisible = false;

                ServidorBancoGrid.IsVisible = true;
                CredenciaisGrid.IsVisible = true;

                ServidorEntry.Placeholder =
                    "Ex: 172.24.32.1 ou localhost";

                BancoEntry.Placeholder =
                    "Ex: db_academia_do_ze";

                UsuarioEntry.Placeholder =
                    "Ex: sa";

                ComplementoLabel.Text =
                    "Complemento (SSL / Timeout / Criptografia)";

                ComplementoEntry.Placeholder =
                    "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;";

                ServidorEntry.Text =
                    Preferences.Get(
                        "SqlServer_Servidor",
                        ServidorPadraoSqlServer);

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
                        SenhaPadrao);

                ComplementoEntry.Text =
                    Preferences.Get(
                        "SqlServer_Complemento",
                        "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;");

                break;

            // =================================================
            // MYSQL
            // =================================================

            case AppDatabaseType.MySql:

                SqliteInfoCard.IsVisible = false;
                SqliteContainer.IsVisible = false;

                ServidorBancoGrid.IsVisible = true;
                CredenciaisGrid.IsVisible = true;

                // No Android Emulator:
                // 10.0.2.2 aponta para o computador Windows.
                ServidorEntry.Placeholder =
                    "Ex: 10.0.2.2 ou localhost";

                BancoEntry.Placeholder =
                    "Ex: db_academia_do_ze";

                UsuarioEntry.Placeholder =
                    "Ex: root";

                ComplementoLabel.Text =
                    "Complemento (Porta / Timeout)";

                ComplementoEntry.Placeholder =
                    "Port=3306;Connection Timeout=5;Default Command Timeout=30;";

                ServidorEntry.Text =
                    Preferences.Get(
                        "MySql_Servidor",
                        ServidorPadraoMySql);

                BancoEntry.Text =
                    Preferences.Get(
                        "MySql_Banco",
                        "db_academia_do_ze");

                UsuarioEntry.Text =
                    Preferences.Get(
                        "MySql_Usuario",
                        "root");

                SenhaEntry.Text =
                    Preferences.Get(
                        "MySql_Senha",
                        SenhaPadrao);

                ComplementoEntry.Text =
                    Preferences.Get(
                        "MySql_Complemento",
                        "Port=3306;Connection Timeout=5;Default Command Timeout=30;");

                break;
        }
    }

    // =========================================================
    // SALVAR BANCO DE DADOS
    // =========================================================

    private async void OnSalvarBdClicked(
        object? sender,
        EventArgs e)
    {
        if (DatabaseTypePicker.SelectedItem is not string selectedTypeStr ||
            !Enum.TryParse<AppDatabaseType>(
                selectedTypeStr,
                out var selectedType))
        {
            await DisplayAlertAsync(
                "Aviso",
                "Selecione um tipo de banco de dados válido.",
                "OK");

            return;
        }

        // =====================================================
        // SQLITE
        // =====================================================

        if (selectedType == AppDatabaseType.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(
                    SqliteCaminhoEntry.Text))
            {
                await DisplayAlertAsync(
                    "Validação",
                    "Informe o caminho do arquivo do banco SQLite.",
                    "OK");

                return;
            }

            var caminho =
                SqliteCaminhoEntry.Text.Trim();

            var complemento =
                ComplementoEntry.Text?.Trim()
                ?? string.Empty;

            Preferences.Set(
                "Sqlite_Caminho",
                caminho);

            Preferences.Set(
                "Sqlite_Complemento",
                complemento);
        }

        // =====================================================
        // MYSQL / SQL SERVER
        // =====================================================

        else
        {
            if (string.IsNullOrWhiteSpace(
                    ServidorEntry.Text))
            {
                await DisplayAlertAsync(
                    "Validação",
                    "Informe o servidor do banco de dados.",
                    "OK");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    BancoEntry.Text))
            {
                await DisplayAlertAsync(
                    "Validação",
                    "Informe o nome do banco de dados.",
                    "OK");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    UsuarioEntry.Text))
            {
                await DisplayAlertAsync(
                    "Validação",
                    "Informe o usuário do banco de dados.",
                    "OK");

                return;
            }

            var servidor =
                ServidorEntry.Text.Trim();

            var banco =
                BancoEntry.Text.Trim();

            var usuario =
                UsuarioEntry.Text.Trim();

            var senha =
                SenhaEntry.Text ?? string.Empty;

            var complemento =
                ComplementoEntry.Text?.Trim()
                ?? string.Empty;

            var prefix =
                selectedType == AppDatabaseType.SqlServer
                    ? "SqlServer"
                    : "MySql";

            Preferences.Set(
                $"{prefix}_Servidor",
                servidor);

            Preferences.Set(
                $"{prefix}_Banco",
                banco);

            Preferences.Set(
                $"{prefix}_Usuario",
                usuario);

            Preferences.Set(
                $"{prefix}_Senha",
                senha);

            Preferences.Set(
                $"{prefix}_Complemento",
                complemento);
        }

        // Salva o tipo do banco.
        Preferences.Set(
            "DatabaseType",
            selectedType.ToString());

        // Informa ao ConfigurationHelper
        // que a configuração mudou.
        WeakReferenceMessenger.Default.Send(
            new BancoPreferencesUpdatedMessage(
                "BancoAlterado"));

        await DisplayAlertAsync(
            "Sucesso",
            $"Configurações do banco de dados ({selectedType}) salvas com sucesso!",
            "OK");

        await Shell.Current.GoToAsync("//dashboard");
    }

    // =========================================================
    // CANCELAR
    // =========================================================

    private async void OnCancelarClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("//dashboard");
    }

    // =========================================================
    // LIMPEZA
    // =========================================================

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Remove as mensagens registradas nesta página.
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}