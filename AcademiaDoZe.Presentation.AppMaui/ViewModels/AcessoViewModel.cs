// Jonathan de Souza Pereira
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Exceptions;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

// Pessoa (aluno ou colaborador) exibida no seletor
public class PessoaAcessoItem
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public ArquivoDto? Foto { get; set; }

    // texto exibido no Picker
    public override string ToString() => $"{Nome} ({Cpf})";
}

// Linha da lista "Acessos Registrados Hoje"
public class AcessoHojeItem
{
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Hora { get; set; } = string.Empty;
    public string Inicial => string.IsNullOrWhiteSpace(Nome) ? "?" : Nome.Trim()[..1].ToUpperInvariant();
    public DateTime Ordem { get; set; }
}

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IAcessoAlunoService _acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;

    public ObservableCollection<PessoaAcessoItem> Pessoas { get; } = [];
    public ObservableCollection<AcessoHojeItem> AcessosHoje { get; } = [];

    // ── Modo: Aluno / Colaborador ────────────────────────────────────────────
    private bool _isAlunoMode = true;
    public bool IsAlunoMode
    {
        get => _isAlunoMode;
        set
        {
            if (SetProperty(ref _isAlunoMode, value))
            {
                OnPropertyChanged(nameof(ModoAtualTexto));
                OnPropertyChanged(nameof(RotuloSelecao));
            }
        }
    }
    public string ModoAtualTexto => $"Modo atual selecionado: {(IsAlunoMode ? "Aluno" : "Colaborador")}";
    public string RotuloSelecao => IsAlunoMode ? "Selecione o Aluno:" : "Selecione o Colaborador:";

    // ── Busca ────────────────────────────────────────────────────────────────
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private PessoaAcessoItem? _selectedPessoa;
    public PessoaAcessoItem? SelectedPessoa
    {
        get => _selectedPessoa;
        set
        {
            if (SetProperty(ref _selectedPessoa, value))
            {
                OnPropertyChanged(nameof(HasSelecionado));
                _ = AtualizarStatusAsync();
            }
        }
    }
    public bool HasSelecionado => SelectedPessoa != null;

    // ── Status da pessoa selecionada ─────────────────────────────────────────
    private bool _dentroDaAcademia;
    public bool DentroDaAcademia
    {
        get => _dentroDaAcademia;
        set
        {
            if (SetProperty(ref _dentroDaAcademia, value))
            {
                OnPropertyChanged(nameof(StatusTexto));
                OnPropertyChanged(nameof(TextoBotaoRegistro));
            }
        }
    }
    public string StatusTexto => DentroDaAcademia ? "DENTRO DA ACADEMIA" : "FORA DA ACADEMIA";
    public string TextoBotaoRegistro => DentroDaAcademia ? "Registrar Saída" : "Registrar Entrada";

    // ── Resultado da última operação ─────────────────────────────────────────
    private bool _mostrarSucesso;
    public bool MostrarSucesso
    {
        get => _mostrarSucesso;
        set => SetProperty(ref _mostrarSucesso, value);
    }

    private bool _mostrarErro;
    public bool MostrarErro
    {
        get => _mostrarErro;
        set => SetProperty(ref _mostrarErro, value);
    }

    private string _resultadoTitulo = string.Empty;
    public string ResultadoTitulo
    {
        get => _resultadoTitulo;
        set => SetProperty(ref _resultadoTitulo, value);
    }

    private string _resultadoMensagem = string.Empty;
    public string ResultadoMensagem
    {
        get => _resultadoMensagem;
        set => SetProperty(ref _resultadoMensagem, value);
    }

    public AcessoViewModel(
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        IAcessoAlunoService acessoAlunoService,
        IAcessoColaboradorService acessoColaboradorService)
    {
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _acessoAlunoService = acessoAlunoService;
        _acessoColaboradorService = acessoColaboradorService;
        Title = "Controle de Acesso";
    }

    // Chamado pela página no OnAppearing
    public async Task InitializeAsync()
    {
        await LoadAcessosHojeAsync();
        await AtualizarStatusAsync();
    }

    private void LimparResultado()
    {
        MostrarSucesso = false;
        MostrarErro = false;
        ResultadoTitulo = string.Empty;
        ResultadoMensagem = string.Empty;
    }

    private void ExibirResultado(bool sucesso, string titulo, string mensagem)
    {
        ResultadoTitulo = titulo;
        ResultadoMensagem = mensagem;
        MostrarSucesso = sucesso;
        MostrarErro = !sucesso;
    }

    private void LimparSelecao()
    {
        Pessoas.Clear();
        SearchText = string.Empty;
        SelectedPessoa = null;
        DentroDaAcademia = false;
        LimparResultado();
    }

    // ── Comandos ─────────────────────────────────────────────────────────────
    [RelayCommand]
    private void SelecionarModoAluno()
    {
        IsAlunoMode = true;
        LimparSelecao();
    }

    [RelayCommand]
    private void SelecionarModoColaborador()
    {
        IsAlunoMode = false;
        LimparSelecao();
    }

    // Localiza por nome ou CPF
    [RelayCommand]
    private async Task SearchPessoaAsync()
    {
        if (IsBusy) return;

        var texto = SearchText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(texto))
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Digite o nome ou o CPF para localizar.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            LimparResultado();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var digitos = new string(texto.Where(char.IsDigit).ToArray());

            var encontrados = new List<PessoaAcessoItem>();

            if (IsAlunoMode)
            {
                var porNome = await _alunoService.ObterPorNomeAsync(texto, cts.Token) ?? [];
                encontrados.AddRange(porNome.Select(a => new PessoaAcessoItem { Id = a.Id, Nome = a.Nome, Cpf = a.Cpf, Foto = a.Foto }));

                if (digitos.Length > 0)
                {
                    var porCpf = await _alunoService.ObterPorCpfAsync(digitos, cts.Token);
                    if (porCpf != null && encontrados.All(p => p.Id != porCpf.Id))
                        encontrados.Add(new PessoaAcessoItem { Id = porCpf.Id, Nome = porCpf.Nome, Cpf = porCpf.Cpf, Foto = porCpf.Foto });

                    // busca parcial por CPF (ex: "333")
                    var todos = await _alunoService.ObterTodosAsync(cts.Token) ?? [];
                    foreach (var a in todos.Where(a => a.Cpf.Contains(digitos) && encontrados.All(p => p.Id != a.Id)))
                        encontrados.Add(new PessoaAcessoItem { Id = a.Id, Nome = a.Nome, Cpf = a.Cpf, Foto = a.Foto });
                }
            }
            else
            {
                var todos = await _colaboradorService.ObterTodosAsync(cts.Token) ?? [];
                var filtrados = todos.Where(c =>
                    c.Nome.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    (digitos.Length > 0 && c.Cpf.Contains(digitos)));
                encontrados.AddRange(filtrados.Select(c => new PessoaAcessoItem { Id = c.Id, Nome = c.Nome, Cpf = c.Cpf, Foto = c.Foto }));
            }

            Pessoas.Clear();
            foreach (var p in encontrados.OrderBy(p => p.Nome))
                Pessoas.Add(p);

            SelectedPessoa = Pessoas.FirstOrDefault();

            if (Pessoas.Count == 0)
                await Shell.Current.DisplayAlertAsync("Aviso", IsAlunoMode ? "Nenhum aluno encontrado." : "Nenhum colaborador encontrado.", "OK");
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync("Tempo Esgotado", "A busca expirou. Verifique a conexão com o banco.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao localizar pessoa: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Consulta se a pessoa selecionada está dentro da academia (define entrada x saída)
    private async Task AtualizarStatusAsync()
    {
        try
        {
            if (SelectedPessoa == null)
            {
                DentroDaAcademia = false;
                return;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            DentroDaAcademia = IsAlunoMode
                ? await _acessoAlunoService.EstaNaAcademiaAsync(SelectedPessoa.Id, cts.Token)
                : await _acessoColaboradorService.EstaNaAcademiaAsync(SelectedPessoa.Id, cts.Token);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao consultar o status de acesso: {ex.Message}", "OK");
        }
    }

    // Botão principal: registra ENTRADA se está fora, SAÍDA se está dentro
    [RelayCommand]
    private async Task RegistrarAcessoAsync()
    {
        if (IsBusy || SelectedPessoa == null) return;

        try
        {
            IsBusy = true;
            LimparResultado();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool saida = DentroDaAcademia;
            string mensagem;

            if (IsAlunoMode)
            {
                var dto = saida
                    ? await _acessoAlunoService.RegistrarSaidaAsync(SelectedPessoa.Id, cts.Token)
                    : await _acessoAlunoService.RegistrarEntradaAsync(SelectedPessoa.Id, cts.Token);
                mensagem = dto.Mensagem ?? string.Empty;
            }
            else
            {
                var dto = saida
                    ? await _acessoColaboradorService.RegistrarSaidaAsync(SelectedPessoa.Id, cts.Token)
                    : await _acessoColaboradorService.RegistrarEntradaAsync(SelectedPessoa.Id, cts.Token);
                mensagem = dto.Mensagem ?? string.Empty;
            }

            ExibirResultado(true, saida ? "Saída Registrada" : "Entrada Autorizada", mensagem);
            await AtualizarStatusAsync();
            await LoadAcessosHojeAsync();
        }
        catch (AcessoNegadoException ex)
        {
            ExibirResultado(false, "Acesso Negado", ex.Message);
            await AtualizarStatusAsync();
        }
        catch (OperationCanceledException)
        {
            ExibirResultado(false, "Tempo Esgotado", "O registro expirou. Verifique a conexão com o banco.");
        }
        catch (Exception ex)
        {
            ExibirResultado(false, "Erro", $"Erro ao registrar acesso: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // "Acessos Registrados Hoje" (alunos + colaboradores)
    [RelayCommand]
    private async Task LoadAcessosHojeAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var itens = new List<AcessoHojeItem>();

            var alunos = await _acessoAlunoService.ObterDoDiaAsync(cts.Token) ?? [];
            foreach (var a in alunos)
                itens.Add(CriarItem(a.Aluno?.Nome, "Aluno", a.Entrada, a.Saida));

            var colaboradores = await _acessoColaboradorService.ObterDoDiaAsync(cts.Token) ?? [];
            foreach (var c in colaboradores)
                itens.Add(CriarItem(c.Colaborador?.Nome, "Colaborador", c.Entrada, c.Saida));

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AcessosHoje.Clear();
                foreach (var item in itens.OrderByDescending(i => i.Ordem))
                    AcessosHoje.Add(item);
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar os acessos de hoje: {ex.Message}", "OK");
        }
    }

    private static AcessoHojeItem CriarItem(string? nome, string tipo, DateTime entrada, DateTime? saida)
    {
        string descricao = saida.HasValue
            ? $"{tipo} • Entrada {entrada:HH:mm:ss} • Saída {saida.Value:HH:mm:ss} • Permanência {Formatar(saida.Value - entrada)}"
            : $"{tipo} • Entrada {entrada:HH:mm:ss} • Dentro da academia";

        return new AcessoHojeItem
        {
            Nome = nome ?? "(sem nome)",
            Tipo = tipo,
            Descricao = descricao,
            Hora = (saida ?? entrada).ToString("HH:mm:ss"),
            Ordem = saida ?? entrada
        };
    }

    private static string Formatar(TimeSpan t) => $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
}