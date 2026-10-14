
// Jonathan de Souza Pereira

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AcademiaDoZe.Application.DTOs;
using Microsoft.Maui.Controls;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels
{
    public class AlunoListViewModel : INotifyPropertyChanged
    {
        private bool isRefreshing;

        public ObservableCollection<AlunoDto> Alunos { get; } = new();

        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                if (isRefreshing != value)
                {
                    isRefreshing = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoadAlunosCommand { get; }
        public ICommand EditAlunoCommand { get; }
        public ICommand DeleteAlunoCommand { get; }

        public AlunoListViewModel()
        {
            LoadAlunosCommand = new Command(
                async () => await LoadAlunosAsync());

            EditAlunoCommand = new Command<AlunoDto>(
                async aluno => await EditAlunoAsync(aluno));

            DeleteAlunoCommand = new Command<AlunoDto>(
                async aluno => await DeleteAlunoAsync(aluno));
        }

        private async Task LoadAlunosAsync()
        {
            try
            {
                IsRefreshing = true;

                // Dados temporários para teste.
                await Task.Delay(500);

                if (Alunos.Count == 0)
                {
                    Alunos.Add(new AlunoDto { Nome = "João da Silva" });
                    Alunos.Add(new AlunoDto { Nome = "Maria Oliveira" });
                    Alunos.Add(new AlunoDto { Nome = "Carlos Souza" });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Erro",
                    $"Não foi possível carregar os alunos: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task EditAlunoAsync(AlunoDto? aluno)
        {
            if (aluno is null)
                return;

            await Shell.Current.DisplayAlert(
                "Editar aluno",
                $"Aluno selecionado: {aluno.Nome}",
                "OK");
        }

        private async Task DeleteAlunoAsync(AlunoDto? aluno)
        {
            if (aluno is null)
                return;

            bool confirmar = await Shell.Current.DisplayAlert(
                "Excluir aluno",
                $"Deseja excluir {aluno.Nome}?",
                "Sim",
                "Não");

            if (confirmar)
                Alunos.Remove(aluno);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}