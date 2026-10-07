// Jonathan de Souza Pereira

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public class AppAlertPopup : Popup
{
    public AppAlertPopup(
        string titulo,
        string mensagem)
    {
        // ========================================================
        // CONFIGURAÇÃO DO POPUP
        // ========================================================

        CanBeDismissedByTappingOutsideOfPopup = false;

        HorizontalOptions = LayoutOptions.Center;

        VerticalOptions = LayoutOptions.Center;

        Padding = 0;


        // ========================================================
        // TÍTULO
        // ========================================================

        var tituloLabel = new Label
        {
            Text = titulo,

            FontSize = 20,

            FontAttributes = FontAttributes.Bold,

            HorizontalTextAlignment =
                TextAlignment.Center,

            TextColor =
                Color.FromArgb("#1A1A1A")
        };


        // ========================================================
        // LINHA LARANJA
        // ========================================================

        var linha = new BoxView
        {
            HeightRequest = 2,

            BackgroundColor =
                Color.FromArgb("#E8500A")
        };


        // ========================================================
        // MENSAGEM
        // ========================================================

        var mensagemLabel = new Label
        {
            Text = mensagem,

            FontSize = 15,

            HorizontalTextAlignment =
                TextAlignment.Center,

            TextColor =
                Color.FromArgb("#424242")
        };


        // ========================================================
        // BOTÃO OK
        // ========================================================

        var botaoOk = new Button
        {
            Text = "OK",

            HeightRequest = 46,

            CornerRadius = 12,

            BackgroundColor =
                Color.FromArgb("#E8500A"),

            TextColor =
                Colors.White,

            FontAttributes =
                FontAttributes.Bold
        };


        // ========================================================
        // EVENTO DO BOTÃO
        // ========================================================

        botaoOk.Clicked += async (_, _) =>
        {
            await CloseAsync();
        };


        // ========================================================
        // LAYOUT
        // ========================================================

        var layout = new VerticalStackLayout
        {
            Spacing = 18,

            Children =
            {
                tituloLabel,

                linha,

                mensagemLabel,

                botaoOk
            }
        };


        // ========================================================
        // BORDA
        // ========================================================

        var border = new Border
        {
            WidthRequest = 320,

            Padding = 24,

            BackgroundColor =
                Color.FromArgb("#FFFFFF"),

            Stroke =
                Color.FromArgb("#E0E0E0"),

            StrokeThickness = 1,

            StrokeShape =
                new RoundRectangle
                {
                    CornerRadius =
                        new CornerRadius(20)
                },

            Content = layout
        };


        // ========================================================
        // CONTEÚDO DO POPUP
        // ========================================================

        Content = border;
    }
}