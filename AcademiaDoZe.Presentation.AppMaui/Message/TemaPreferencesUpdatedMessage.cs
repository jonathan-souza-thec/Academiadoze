// Jonathan de Souza Pereira

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AcademiaDoZe.Presentation.AppMaui.Message;

public class TemaPreferencesUpdatedMessage : ValueChangedMessage<string>
{
    public TemaPreferencesUpdatedMessage(string value)
        : base(value)
    {
    }
}