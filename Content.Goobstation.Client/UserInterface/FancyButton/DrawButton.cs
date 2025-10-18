using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.UserInterface.FancyButton;

public sealed class DrawButton : Button
{
    public event Action? OnDrawModeChanged;

    public DrawButton()
    {
    }

    protected override void DrawModeChanged()
    {
        OnDrawModeChanged?.Invoke();
    }
}
