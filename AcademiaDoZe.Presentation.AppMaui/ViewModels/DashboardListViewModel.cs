// Jonathan de Souza Pereira
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel : BaseViewModel
{
    private readonly IAcessoAlunoService _acessoAlunoService;

    public DashboardListViewModel(IAcessoAlunoService acessoAlunoService)
    {
        _acessoAlunoService = acessoAlunoService;
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        // Carregamento dos dados do dashboard
        var acessos = await _acessoAlunoService.ObterTodosAsync();

        // Coloque aqui as propriedades que serão
        // preenchidas com os dados do dashboard.
    }
}