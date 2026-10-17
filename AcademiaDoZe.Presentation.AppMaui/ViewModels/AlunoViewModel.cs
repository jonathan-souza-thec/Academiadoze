// Jonathan de Souza Pereira

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(Id), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;

    private int _id;
    private AlunoDto _alunoAtual = new();

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public AlunoDto AlunoAtual
    {
        get => _alunoAtual;
        set => SetProperty(ref _alunoAtual, value);
    }

    public IAsyncRelayCommand SalvarCommand { get; }
    public IAsyncRelayCommand CancelarCommand { get; }

    public AlunoViewModel(IAlunoService alunoService)
    {
        _alunoService = alunoService;
        Title = "Aluno";

        SalvarCommand = new AsyncRelayCommand(SalvarAsync);
        CancelarCommand = new AsyncRelayCommand(CancelarAsync);
    }

    // Chamado pela AlunoPage no OnAppearing
    public async Task InitializeAsync()
    {
        if (IsBusy)
            return;

        // Novo aluno: formulário em branco
        if (Id <= 0)
        {
            AlunoAtual = new AlunoDto();
            return;
        }

        // Edição: carrega o aluno existente
        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var resultado = await _alunoService.ObterPorIdAsync(Id, cts.Token);

            if (resultado is not null)
                AlunoAtual = resultado;
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo esgotado",
                "O carregamento demorou demais.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Não foi possível carregar o aluno: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SalvarAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            if (AlunoAtual.Id > 0)
                await _alunoService.AtualizarAsync(AlunoAtual, cts.Token);
            else
                await _alunoService.AdicionarAsync(AlunoAtual, cts.Token);

            await Shell.Current.DisplayAlertAsync("Sucesso", "Aluno salvo com sucesso!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo esgotado", "O salvamento demorou demais.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Erro",
                $"Não foi possível salvar o aluno: {ex.Message}",
                "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}