// Jonathan de Souza Pereira
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage
{
    public MatriculaPage(MatriculaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MatriculaViewModel viewModel)
        {
            await viewModel.InitializeAsync();
            // garante que a data fim seja calculada ao carregar dados existentes
            CalculateFimFromPlanoAndInicio();
        }
    }

    private void PlanoPicker_SelectedIndexChanged(object? sender, EventArgs e) => CalculateFimFromPlanoAndInicio();

    private void InicioDatePicker_DateSelected(object? sender, DateChangedEventArgs e) => CalculateFimFromPlanoAndInicio();

    // Data fim = início + duração do plano
    private void CalculateFimFromPlanoAndInicio()
    {
        if (BindingContext is not MatriculaViewModel vm) return;

        var inicio = vm.Matricula.DataInicio;
        DateOnly fim = vm.Matricula.Plano switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => inicio,
        };

        vm.Matricula.DataFim = fim;

        // atualiza o DatePicker somente leitura
        try { FimDatePicker.Date = fim.ToDateTime(TimeOnly.MinValue); }
        catch { }
    }
}