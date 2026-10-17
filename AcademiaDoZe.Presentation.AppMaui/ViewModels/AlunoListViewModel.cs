// Jonathan de Souza Pereira

using System.Collections.ObjectModel;
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AlunoListViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    private bool _isRefreshing;

    public ObservableCollection<AlunoDto> Alunos { get; } = [];

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public AlunoListViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = "Alunos";
    }

    [RelayCommand]
    private async Task LoadAlunosAsync()
    {
        if (IsBusy)
        {
            IsRefreshing = false;
            return;
        }

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var alunos = await _alunoService.ObterTodosAsync(cts.Token);

            Alunos.Clear();

            if (alunos is not null)
            {
                foreach (var aluno in alunos)
                    Alunos.Add(aluno);
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "O carregamento demorou demais. Verifique a conexão com o banco de dados.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Não foi possível carregar os alunos: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task AddAlunoAsync()
    {
        await Shell.Current.GoToAsync("aluno");
    }

    [RelayCommand]
    private async Task EditAlunoAsync(AlunoDto? aluno)
    {
        if (aluno is null)
            return;

        await Shell.Current.GoToAsync($"aluno?Id={aluno.Id}");
    }

    [RelayCommand]
    private async Task DeleteAlunoAsync(AlunoDto? aluno)
    {
        if (aluno is null || IsBusy)
            return;

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar exclusão",
            $"Deseja realmente excluir o aluno {aluno.Nome}?",
            "Sim",
            "Não");

        if (!confirmar)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            bool removido = await _alunoService.RemoverAsync(aluno.Id, cts.Token);

            if (removido)
            {
                Alunos.Remove(aluno);

                await Shell.Current.DisplayAlertAsync(
                    "Sucesso",
                    "Aluno excluído com sucesso!",
                    "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    "Aviso",
                    "Não foi possível excluir o aluno.",
                    "OK");
            }
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "A exclusão demorou demais.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Não foi possível excluir o aluno: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAlunosAsync();
    }
}