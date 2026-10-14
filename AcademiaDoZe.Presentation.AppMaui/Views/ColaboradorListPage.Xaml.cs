using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ColaboradorListPage : ContentPage
{
    public ColaboradorListPage(ColaboradorListViewModel viewModel)
    {
        InitializeComponent(); // só funciona se o x:Class do XAML for igual
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ColaboradorListViewModel viewModel)
        {
            await viewModel.LoadColaboradoresCommand.ExecuteAsync(null);
        }
    }

    private async void OnEditButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
            button.BindingContext is ColaboradorDto colaborador &&
            BindingContext is ColaboradorListViewModel viewModel)
        {
            await viewModel.EditColaboradorCommand.ExecuteAsync(colaborador);
        }
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button &&
            button.BindingContext is ColaboradorDto colaborador &&
            BindingContext is ColaboradorListViewModel viewModel)
        {
            await viewModel.DeleteColaboradorCommand.ExecuteAsync(colaborador);
        }
    }
}
