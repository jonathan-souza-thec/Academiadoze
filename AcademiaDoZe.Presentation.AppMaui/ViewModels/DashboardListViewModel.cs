// Jonathan de Souza Pereira

using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel : BaseViewModel
{
    // CORREÇÃO: usa ILogradouroService (a funcionalidade desta etapa).
    // IAcessoAlunoService é da Avaliação 03 e não conta logradouros.
    private readonly ILogradouroService _logradouroService;

    private int _totalLogradouros;

    public int TotalLogradouros
    {
        get => _totalLogradouros;
        set => SetProperty(ref _totalLogradouros, value);
    }

    public DashboardListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;

        Title = "Dashboard";
    }

    [RelayCommand]
    public async Task LoadDashboardDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var logradouros = await _logradouroService.ObterTodosAsync(cts.Token);

            TotalLogradouros = logradouros?.Count() ?? 0;
        }
        catch (Exception ex)
        {
            TotalLogradouros = 0;
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar o dashboard: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // CORREÇÃO: navegação para a lista via rota "logradouros" do AppShell.xaml
    [RelayCommand]
    private async Task NavigateToLogradourosAsync()
    {
        await Shell.Current.GoToAsync("//logradouros");
    }
}