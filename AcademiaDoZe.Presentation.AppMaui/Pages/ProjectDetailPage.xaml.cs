using AcademiaDoZe.Presentation.AppMaui.Models;

namespace AcademiaDoZe.Presentation.AppMaui.Pages
{
    public partial class ProjectDetailPage : ContentPage
    {
        public ProjectDetailPage(ProjectDetailPageModel model)
        {
            InitializeComponent();

            BindingContext = model;
        }
    }
}
