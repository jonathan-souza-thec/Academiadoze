// Jonathan de Souza Pereira

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class LogradouroListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;

    // Lista de logradouros
    public ObservableCollection<LogradouroDto> Logradouros { get; } = new();

    // Comandos de edição e exclusão criados explicitamente
    public IAsyncRelayCommand<LogradouroDto> EditLogradouroCommand { get; }

    public IAsyncRelayCommand<LogradouroDto> DeleteLogradouroCommand { get; }

    // Texto da pesquisa
    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    // Estado de atualização da lista
    private bool _isRefreshing;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    // Opções do filtro
    public ObservableCollection<string> FilterTypes { get; } = new()
    {
        "Nome",
        "Cidade",
        "CEP",
        "ID"
    };

    private string _selectedFilterType = "Nome";

    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    // Construtor
    public LogradouroListViewModel(ILogradouroService logradouroService)
    {
        _logradouroService = logradouroService;
        Title = "Logradouros";

        EditLogradouroCommand =
            new AsyncRelayCommand<LogradouroDto>(EditLogradouroAsync);

        DeleteLogradouroCommand =
            new AsyncRelayCommand<LogradouroDto>(DeleteLogradouroAsync);
    }

    // Carregar logradouros
    [RelayCommand]
    private async Task LoadLogradourosAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

            var resultado = await _logradouroService.ObterTodosAsync(
                cts.Token);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Logradouros.Clear();

                foreach (var item in resultado)
                    Logradouros.Add(item);
            });
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "O carregamento dos logradouros expirou.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao carregar logradouros: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    // Pesquisar logradouros
    [RelayCommand]
    private async Task SearchLogradourosAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadLogradourosAsync();
            return;
        }

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

            var resultado = await _logradouroService.ObterTodosAsync(
                cts.Token);

            string termo = SearchText.Trim();

            var encontrados = resultado.Where(l =>
            {
                string valor = SelectedFilterType switch
                {
                    "Cidade" => l.Cidade,
                    "CEP" => l.Cep,
                    "ID" => l.Id.ToString(),
                    _ => l.Nome
                };

                return valor.Contains(
                    termo,
                    StringComparison.OrdinalIgnoreCase);
            }).ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Logradouros.Clear();

                foreach (var item in encontrados)
                    Logradouros.Add(item);
            });

            if (encontrados.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Pesquisa",
                    "Nenhum logradouro encontrado.",
                    "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "A pesquisa demorou mais do que o esperado.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao pesquisar: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Atualizar lista
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
            return;

        IsRefreshing = true;
        await LoadLogradourosAsync();
    }

    // Abrir tela para adicionar
    [RelayCommand]
    private async Task AddLogradouroAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("logradouro");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Não foi possível abrir o cadastro: {ex.Message}",
                "OK");
        }
    }

    // Abrir tela para editar
    private async Task EditLogradouroAsync(LogradouroDto logradouro)
    {
        if (logradouro is null)
            return;

        try
        {
            await Shell.Current.GoToAsync($"logradouro?Id={logradouro.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Não foi possível abrir a edição: {ex.Message}",
                "OK");
        }
    }

    // Excluir logradouro
    private async Task DeleteLogradouroAsync(LogradouroDto logradouro)
    {
        if (logradouro is null || IsBusy)
            return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar exclusão",
            $"Deseja excluir o logradouro '{logradouro.Nome}'?",
            "Excluir",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            IsBusy = true;

            bool removido = await _logradouroService.RemoverAsync(
                logradouro.Id);

            if (removido)
            {
                Logradouros.Remove(logradouro);

                await Shell.Current.DisplayAlertAsync(
                    "Sucesso",
                    "Logradouro excluído com sucesso.",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "Aviso",
                    "O logradouro não foi excluído.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Erro ao excluir logradouro: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}