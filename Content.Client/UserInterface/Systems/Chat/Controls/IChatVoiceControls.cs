using Robust.Client.UserInterface;

namespace Content.Client.UserInterface.Systems.Chat.Controls;

public interface IChatVoiceControls
{
    IEnumerable<Control> CreateControls();
}
