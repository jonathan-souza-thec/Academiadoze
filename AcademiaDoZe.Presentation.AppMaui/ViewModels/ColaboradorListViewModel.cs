using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorListViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;

    // ✅ Propriedade que estava faltando
    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public ObservableCollection<string> FilterTypes { get; } = ["Nome", "CPF", "Id", "Email", "Tipo", "Vínculo"];

    private ObservableCollection<ColaboradorDto> _colaboradores = [];
    public ObservableCollection<ColaboradorDto> Colaboradores
    {
        get => _colaboradores;
        set => SetProperty(ref _colaboradores, value);
    }

    private ColaboradorDto? _selectedColaborador;
    public ColaboradorDto? SelectedColaborador
    {
        get => _selectedColaborador;
        set => SetProperty(ref _selectedColaborador, value);
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    public ColaboradorListViewModel(IColaboradorService colaboradorService)
    {
        _colaboradorService = colaboradorService;
        Title = "Colaboradores";
    }

    [RelayCommand]
    private async Task AddColaboradorAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("colaborador");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EditColaboradorAsync(ColaboradorDto colaborador)
    {
        try
        {
            if (colaborador == null)
                return;
            await Shell.Current.GoToAsync($"colaborador?Id={colaborador.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de edição: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadColaboradoresAsync();
    }

    [RelayCommand]
    private async Task SearchColaboradoresAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            await MainThread.InvokeOnMainThreadAsync(() => { Colaboradores.Clear(); });
            IEnumerable<ColaboradorDto> resultados = [];

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // ... filtros de busca (Nome, CPF, Id, Email, Tipo, Vínculo)
            // [mantém o código que você já tinha]

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var item in resultados)
                {
                    Colaboradores.Add(item);
                }
                OnPropertyChanged(nameof(Colaboradores));
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar colaboradores: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadColaboradoresAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Colaboradores.Clear();
                OnPropertyChanged(nameof(Colaboradores));
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var colaboradoresList = await _colaboradorService.ObterTodosAsync(cts.Token);

            if (colaboradoresList != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var colaborador in colaboradoresList)
                    {
                        Colaboradores.Add(colaborador);
                    }
                    OnPropertyChanged(nameof(Colaboradores));
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar colaboradores: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteColaboradorAsync(ColaboradorDto colaborador)
    {
        if (colaborador == null)
            return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o colaborador {colaborador.Nome}?",
            "Sim", "Não");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool success = await _colaboradorService.RemoverAsync(colaborador.Id, cts.Token);

            if (success)
            {
                Colaboradores.Remove(colaborador);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Colaborador excluído com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir o colaborador.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir colaborador: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
