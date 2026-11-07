// Jonathan de Souza Pereira

using AcademiaDoZe.Application.Common;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel : BaseViewModel
{
    private static readonly Color CorSucesso = Color.FromArgb("#0F5132");
    private static readonly Color CorErro = Color.FromArgb("#842029");

    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IAcessoAlunoService _acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;
    private readonly DemoDataSeeder _seeder;

    private List<PessoaItem> _todasPessoas = [];
    private int _acessoAbertoId;
    private bool _atualizandoLista;

    public ObservableCollection<PessoaItem> Pessoas { get; } = [];
    public ObservableCollection<RegistroHojeItem> RegistrosHoje { get; } = [];

    public AcessoViewModel(
        IAlunoService alunoService,
        IColaboradorService colaboradorService,
        IAcessoAlunoService acessoAlunoService,
        IAcessoColaboradorService acessoColaboradorService,
        DemoDataSeeder seeder)
    {
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _acessoAlunoService = acessoAlunoService;
        _acessoColaboradorService = acessoColaboradorService;
        _seeder = seeder;
        Title = "Controle de Acesso";
    }

    // ── Modo (Aluno / Colaborador) ────────────────────────────────────────
    private bool _modoAluno = true;
    public bool ModoAluno
    {
        get => _modoAluno;
        set
        {
            if (SetProperty(ref _modoAluno, value))
            {
                OnPropertyChanged(nameof(ModoTexto));
                OnPropertyChanged(nameof(RotuloSelecao));
            }
        }
    }
    public string ModoTexto => ModoAluno ? "Modo atual selecionado: Aluno" : "Modo atual selecionado: Colaborador";
    public string RotuloSelecao => ModoAluno ? "Selecione o Aluno:" : "Selecione o Colaborador:";

    // ── Busca / seleção ───────────────────────────────────────────────────
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                AplicarFiltro();
        }
    }

    private PessoaItem? _pessoaSelecionada;
    public PessoaItem? PessoaSelecionada
    {
        get => _pessoaSelecionada;
        set
        {
            if (SetProperty(ref _pessoaSelecionada, value))
            {
                OnPropertyChanged(nameof(TemPessoa));
                if (!_atualizandoLista)
                    _ = AtualizarStatusAsync();
            }
        }
    }
    public bool TemPessoa => PessoaSelecionada != null;

    // ── Status dentro/fora ────────────────────────────────────────────────
    private bool _pessoaDentro;
    public bool PessoaDentro
    {
        get => _pessoaDentro;
        set
        {
            if (SetProperty(ref _pessoaDentro, value))
            {
                OnPropertyChanged(nameof(StatusTexto));
                OnPropertyChanged(nameof(BotaoTexto));
            }
        }
    }
    public string StatusTexto => PessoaDentro ? "DENTRO DA ACADEMIA" : "FORA DA ACADEMIA";
    public string BotaoTexto => PessoaDentro ? "Registrar Saída" : "Registrar Entrada";

    // ── Mensagem de resultado ─────────────────────────────────────────────
    private string _mensagemTitulo = string.Empty;
    public string MensagemTitulo { get => _mensagemTitulo; set => SetProperty(ref _mensagemTitulo, value); }

    private string _mensagemTexto = string.Empty;
    public string MensagemTexto { get => _mensagemTexto; set => SetProperty(ref _mensagemTexto, value); }

    private bool _mensagemVisivel;
    public bool MensagemVisivel { get => _mensagemVisivel; set => SetProperty(ref _mensagemVisivel, value); }

    private Color _mensagemCor = CorSucesso;
    public Color MensagemCor { get => _mensagemCor; set => SetProperty(ref _mensagemCor, value); }

    private void ExibirResultado(bool sucesso, string titulo, string texto)
    {
        MensagemTitulo = titulo;
        MensagemTexto = texto;
        MensagemCor = sucesso ? CorSucesso : CorErro;
        MensagemVisivel = true;
    }

    // ── Comandos ──────────────────────────────────────────────────────────
    [RelayCommand]
    private async Task InicializarAsync()
    {
        ModoAluno = true;
        await CarregarPessoasAsync();
        await CarregarRegistrosHojeAsync();
    }

    [RelayCommand]
    private async Task SelecionarAlunoAsync()
    {
        ModoAluno = true;
        MensagemVisivel = false;
        await CarregarPessoasAsync();
    }

    [RelayCommand]
    private async Task SelecionarColaboradorAsync()
    {
        ModoAluno = false;
        MensagemVisivel = false;
        await CarregarPessoasAsync();
    }

    [RelayCommand]
    private async Task AtualizarRegistrosAsync() => await CarregarRegistrosHojeAsync();

    [RelayCommand]
    private async Task RegistrarAsync()
    {
        if (IsBusy) return;

        if (PessoaSelecionada == null)
        {
            await Shell.Current.DisplayAlertAsync("Atenção", "Selecione uma pessoa antes de registrar.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            int id = PessoaSelecionada.Id;

            if (ModoAluno)
            {
                if (PessoaDentro)
                {
                    var r = await _acessoAlunoService.RegistrarSaidaAsync(_acessoAbertoId, cts.Token);
                    ExibirResultado(r.Autorizado, r.Autorizado ? "Saída Registrada" : "Saída Negada", r.Mensagem);
                }
                else
                {
                    var r = await _acessoAlunoService.RegistrarEntradaAsync(id, cts.Token);
                    ExibirResultado(r.Autorizado, r.Autorizado ? "Entrada Autorizada" : "Acesso Negado", r.Mensagem);
                }
            }
            else
            {
                if (PessoaDentro)
                {
                    var r = await _acessoColaboradorService.RegistrarSaidaAsync(_acessoAbertoId, cts.Token);
                    ExibirResultado(r.Autorizado, r.Autorizado ? "Saída Registrada" : "Saída Negada", r.Mensagem);
                }
                else
                {
                    var r = await _acessoColaboradorService.RegistrarEntradaAsync(id, cts.Token);
                    ExibirResultado(r.Autorizado, r.Autorizado ? "Entrada Registrada" : "Acesso Negado", r.Mensagem);
                }
            }

            await AtualizarStatusAsync();
            await CarregarRegistrosHojeAsync();
        }
        catch (OperationCanceledException)
        {
            ExibirResultado(false, "Tempo Esgotado", "A operação expirou. Verifique a conexão com o banco.");
        }
        catch (Exception ex)
        {
            ExibirResultado(false, "Erro", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CriarDadosDemoAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            string resumo = await _seeder.CriarAsync(cts.Token);
            await Shell.Current.DisplayAlertAsync("Dados de demonstração", resumo, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Não foi possível criar os dados: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
        await CarregarPessoasAsync();
    }

    // ── Carregamento ──────────────────────────────────────────────────────
    private async Task CarregarPessoasAsync()
    {
        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            if (ModoAluno)
            {
                var alunos = await _alunoService.ObterTodosAsync(cts.Token);
                _todasPessoas = alunos.Select(a => new PessoaItem(a.Id, a.Nome, a.Cpf)).OrderBy(p => p.Nome).ToList();
            }
            else
            {
                var colaboradores = await _colaboradorService.ObterTodosAsync(cts.Token);
                _todasPessoas = colaboradores.Select(c => new PessoaItem(c.Id, c.Nome, c.Cpf)).OrderBy(p => p.Nome).ToList();
            }

            if (SearchText.Length != 0)
                SearchText = string.Empty;   // dispara AplicarFiltro()
            else
                AplicarFiltro();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar pessoas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void AplicarFiltro()
    {
        _atualizandoLista = true;
        try
        {
            string texto = (SearchText ?? string.Empty).Trim();
            string digitos = SomenteDigitos(texto);

            IEnumerable<PessoaItem> filtradas = _todasPessoas;
            if (texto.Length > 0)
            {
                filtradas = _todasPessoas.Where(p =>
                    p.Nome.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    (digitos.Length > 0 && SomenteDigitos(p.Cpf).Contains(digitos)));
            }

            Pessoas.Clear();
            foreach (var p in filtradas)
                Pessoas.Add(p);

            PessoaSelecionada = Pessoas.FirstOrDefault();
        }
        finally
        {
            _atualizandoLista = false;
        }

        _ = AtualizarStatusAsync();
    }

    private static string SomenteDigitos(string s) => new(s.Where(char.IsDigit).ToArray());

    private async Task AtualizarStatusAsync()
    {
        try
        {
            if (PessoaSelecionada == null)
            {
                _acessoAbertoId = 0;
                PessoaDentro = false;
                return;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            if (ModoAluno)
            {
                var aberto = await _acessoAlunoService.ObterAcessoAbertoAsync(PessoaSelecionada.Id, cts.Token);
                _acessoAbertoId = aberto?.Id ?? 0;
                PessoaDentro = aberto != null;
            }
            else
            {
                var aberto = await _acessoColaboradorService.ObterAcessoAbertoAsync(PessoaSelecionada.Id, cts.Token);
                _acessoAbertoId = aberto?.Id ?? 0;
                PessoaDentro = aberto != null;
            }
        }
        catch (Exception ex)
        {
            ExibirResultado(false, "Erro", $"Erro ao consultar status: {ex.Message}");
        }
    }

    private async Task CarregarRegistrosHojeAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var itens = new List<RegistroHojeItem>();

            foreach (var a in await _acessoAlunoService.ObterDeHojeAsync(cts.Token))
            {
                string nome = a.Aluno?.Nome ?? $"Aluno #{a.Id}";
                itens.Add(new RegistroHojeItem(nome, "Aluno • Entrada registrada.", a.Entrada));
                if (a.Saida.HasValue)
                    itens.Add(new RegistroHojeItem(nome,
                        $"Aluno • Saída registrada. Tempo de permanência: {a.TempoPermanencia!.Value.ToHoraMinutoSegundo()}.",
                        a.Saida.Value));
            }

            foreach (var c in await _acessoColaboradorService.ObterDeHojeAsync(cts.Token))
            {
                string nome = c.Colaborador?.Nome ?? $"Colaborador #{c.Id}";
                itens.Add(new RegistroHojeItem(nome, "Colaborador • Entrada registrada.", c.Entrada));
                if (c.Saida.HasValue)
                    itens.Add(new RegistroHojeItem(nome,
                        $"Colaborador • Saída registrada. Permanência: {c.TempoPermanencia!.Value.ToHoraMinutoSegundo()}.",
                        c.Saida.Value));
            }

            RegistrosHoje.Clear();
            foreach (var item in itens.OrderByDescending(i => i.Hora))
                RegistrosHoje.Add(item);
        }
        catch (Exception ex)
        {
            ExibirResultado(false, "Erro", $"Erro ao carregar acessos de hoje: {ex.Message}");
        }
    }
}