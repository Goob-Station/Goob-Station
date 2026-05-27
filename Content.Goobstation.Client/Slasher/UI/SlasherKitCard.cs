using Content.Goobstation.Shared.Slasher.UI;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.Slasher.UI;

/// <summary>
/// A kit card for the Slasher kit selection menu.
/// </summary>
public sealed class SlasherKitCard : Control
{
    public readonly Button SelectButton;

    public SlasherKitCard(SlasherKitInfo kit, SpriteSystem spriteSystem)
    {
        var panel = new PanelContainer
        {
            Margin = new Thickness(4),
            MinWidth = 180,
            MaxWidth = 180,
            MinHeight = 300,
            MaxHeight = 300,
            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#18211b") }
        };

        var column = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            Margin = new Thickness(6)
        };

        var icon = new TextureRect
        {
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            HorizontalAlignment = HAlignment.Center,
            SetSize = new System.Numerics.Vector2(64, 64),
            Margin = new Thickness(4),
            Texture = spriteSystem.Frame0(kit.Sprite)
        };

        var separator = new PanelContainer
        {
            Margin = new Thickness(0, 4),
            MinHeight = 1,
            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#404040") }
        };

        var name = new Label
        {
            Text = kit.Name,
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 2),
            StyleClasses = { "StatusFieldTitle" }
        };

        var descPanel = new PanelContainer
        {
            Margin = new Thickness(0, 4),
            HorizontalExpand = true,
            VerticalExpand = true,
            MinHeight = 80,
            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#0d1510") }
        };

        var desc = new RichTextLabel
        {
            HorizontalExpand = true,
            Margin = new Thickness(4),
        };

        SelectButton = new Button
        {
            Text = Loc.GetString("slasher-kit-select-button"),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0),
            StyleClasses = { "OpenRight" }
        };

        column.AddChild(icon);
        column.AddChild(separator);
        column.AddChild(name);
        desc.SetMessage(kit.Description);
        descPanel.AddChild(desc);
        column.AddChild(descPanel);
        column.AddChild(SelectButton);
        panel.AddChild(column);
        AddChild(panel);
    }
}
