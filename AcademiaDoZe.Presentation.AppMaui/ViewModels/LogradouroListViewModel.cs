
// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ColaboradorListViewModel : BaseViewModel
{
    private readonly IColaboradorService _colaboradorService;

    private ObservableCollection<ColaboradorDto> _colaboradores = [];

    public ObservableCollection<ColaboradorDto> Colaboradores
    {
        get => _colaboradores;
        set => SetProperty(ref _colaboradores, value);
    }

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private bool _isRefreshing;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public ColaboradorListViewModel(
        IColaboradorService colaboradorService)
    {
        _colaboradorService = colaboradorService;
        Title = "Colaboradores";
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private async Task LoadColaboradoresAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            var resultado =
                await _colaboradorService.ObterTodosAsync(cts.Token);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Colaboradores.Clear();

                foreach (var colaborador in resultado)
                {
                    Colaboradores.Add(colaborador);
                }
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "O carregamento dos colaboradores expirou.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao carregar colaboradores: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private async Task SearchColaboradoresAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadColaboradoresAsync();
            return;
        }

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            var colaborador =
                await _colaboradorService.ObterPorCpfAsync(
                    SearchText.Trim(),
                    cts.Token);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Colaboradores.Clear();

                if (colaborador is not null)
                {
                    Colaboradores.Add(colaborador);
                }
            });

            if (colaborador is null)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Consulta",
                    "Nenhum colaborador encontrado com esse CPF.",
                    "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "A busca expirou. Verifique a conexão com o banco.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao buscar colaborador: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [CommunityToolkit.Mvvm.Input.RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadColaboradoresAsync();
    }
}

