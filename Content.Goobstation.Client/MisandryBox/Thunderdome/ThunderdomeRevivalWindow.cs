using System.Numerics;
using Content.Goobstation.UIKit.UserInterface.Controls;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.MisandryBox.Thunderdome;

public sealed class ThunderdomeRevivalWindow : ThunderdomeWindow
{
    public event Action? OnAccepted;

    public ThunderdomeRevivalWindow()
    {
        WindowTitle = Loc.GetString("thunderdome-revival-title");
        SetSize = new Vector2(350, 160);
        Resizable = false;

        var message = new Label
        {
            Text = Loc.GetString("thunderdome-revival-offer"),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(8, 12),
        };
        Contents.AddChild(message);

        var buttons = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Center,
            SeparationOverride = 8,
            Margin = new Thickness(8, 4),
        };

        var acceptButton = new ThunderdomeButton
        {
            Text = Loc.GetString("thunderdome-revival-accept"),
        };
        acceptButton.OnPressed += () =>
        {
            OnAccepted?.Invoke();
            Close();
        };
        buttons.AddChild(acceptButton);

        var declineButton = new ThunderdomeButton
        {
            Text = Loc.GetString("thunderdome-revival-decline"),
        };
        declineButton.OnPressed += () => Close();
        buttons.AddChild(declineButton);

        Contents.AddChild(buttons);
    }
}
