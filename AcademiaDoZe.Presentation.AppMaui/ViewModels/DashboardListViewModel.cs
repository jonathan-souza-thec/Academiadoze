// Jonathan de Souza Pereira

using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel : BaseViewModel
{
    private readonly IAcessoAlunoService _acessoAlunoService;

    private int _totalAcessos;

    public int TotalAcessos
    {
        get => _totalAcessos;
        set => SetProperty(ref _totalAcessos, value);
    }

    public DashboardListViewModel(
        IAcessoAlunoService acessoAlunoService)
    {
        _acessoAlunoService = acessoAlunoService;

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

            var acessos = await _acessoAlunoService.ObterTodosAsync();

            TotalAcessos = acessos?.Count() ?? 0;
        }
        catch (Exception)
        {
            TotalAcessos = 0;
        }
        finally
        {
            IsBusy = false;
        }
    }
}