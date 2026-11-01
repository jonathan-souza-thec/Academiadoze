// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    private readonly IMatriculaService _matriculaService;

    public ObservableCollection<string> FilterTypes { get; } = ["Id", "Id Aluno", "Ativas", "Vencendo em dias"];

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Ativas";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    private ObservableCollection<MatriculaDto> _matriculas = [];
    public ObservableCollection<MatriculaDto> Matriculas
    {
        get => _matriculas;
        set => SetProperty(ref _matriculas, value);
    }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public MatriculaListViewModel(IMatriculaService matriculaService)
    {
        _matriculaService = matriculaService;
        Title = "Matrículas";
    }

    // Matrícula ativa: hoje está entre início e fim
    private static bool EstaAtiva(MatriculaDto m)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        return m.DataInicio <= hoje && m.DataFim >= hoje;
    }

    [RelayCommand]
    private async Task AddMatriculaAsync()
    {
        try { await Shell.Current.GoToAsync("matricula"); }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EditMatriculaAsync(MatriculaDto? matricula)
    {
        try
        {
            if (matricula == null) return;
            await Shell.Current.GoToAsync($"matricula?Id={matricula.Id}");
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
        await LoadMatriculasAsync();
    }

    private async Task PreencherAsync(IEnumerable<MatriculaDto> itens)
    {
        var lista = itens.ToList();
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Matriculas.Clear();
            foreach (var item in lista)
                Matriculas.Add(item);
            OnPropertyChanged(nameof(Matriculas));
        });
    }

    // Carga inicial: respeita o filtro padrão (Ativas)
    [RelayCommand]
    private async Task LoadMatriculasAsync()
    {
        if (IsBusy)
        {
            IsRefreshing = false;
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var todas = await _matriculaService.ObterTodosAsync(cts.Token) ?? [];
            var lista = SelectedFilterType == "Ativas" ? todas.Where(EstaAtiva) : todas;
            await PreencherAsync(lista);
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "O carregamento das matrículas expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchMatriculasAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            IEnumerable<MatriculaDto> resultados = [];
            var texto = SearchText?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(texto))
            {
                var todas = await _matriculaService.ObterTodosAsync(cts.Token) ?? [];
                resultados = SelectedFilterType == "Ativas" ? todas.Where(EstaAtiva) : todas;
            }
            else if (SelectedFilterType == "Id")
            {
                if (int.TryParse(texto, out int id) && id > 0)
                {
                    var m = await _matriculaService.ObterPorIdAsync(id, cts.Token);
                    if (m != null) resultados = [m];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por ID, informe um número válido.", "OK");
                    return;
                }
            }
            else if (SelectedFilterType == "Id Aluno")
            {
                if (int.TryParse(texto, out int alunoId) && alunoId > 0)
                {
                    resultados = await _matriculaService.ObterPorAlunoAsync(alunoId, cts.Token) ?? [];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por ID do Aluno, informe um número válido.", "OK");
                    return;
                }
            }
            else if (SelectedFilterType == "Ativas")
            {
                // texto opcional = Id do aluno
                if (int.TryParse(texto, out int alunoId) && alunoId > 0)
                {
                    var m = await _matriculaService.ObterMatriculaAtivaPorAlunoAsync(alunoId, cts.Token);
                    if (m != null) resultados = [m];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para filtrar ativas de um aluno, informe o ID do aluno (ou deixe vazio para todas).", "OK");
                    return;
                }
            }
            else if (SelectedFilterType == "Vencendo em dias")
            {
                if (int.TryParse(texto, out int dias) && dias >= 0)
                {
                    resultados = await _matriculaService.ObterVencendoEmDiasAsync(dias, cts.Token) ?? [];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por vencimento, informe a quantidade de dias (número inteiro).", "OK");
                    return;
                }
            }

            await PreencherAsync(resultados);
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca de matrículas expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMatriculaAsync(MatriculaDto? matricula)
    {
        if (matricula == null || IsBusy) return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir a matrícula {matricula.Id}?",
            "Sim", "Não");
        if (!confirmar) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            bool sucesso = await _matriculaService.RemoverAsync(matricula.Id, cts.Token);
            if (sucesso)
            {
                Matriculas.Remove(matricula);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula excluída com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir a matrícula.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A exclusão da matrícula expirou. Verifique a conexão.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}