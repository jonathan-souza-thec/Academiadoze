using AcademiaDoZe.Presentation.AppMaui.Models;
using AcademiaDoZe.Presentation.AppMaui.PageModels;

namespace AcademiaDoZe.Presentation.AppMaui.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}