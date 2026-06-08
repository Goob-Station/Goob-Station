using Content.Goobstation.Shared.Slasher.UI;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Slasher.UI;

/// <summary>
/// A kit card for the Slasher kit selection menu.
/// </summary>
public sealed class SlasherKitCard : Control
{
    public readonly Button SelectButton;

    public SlasherKitCard(SlasherKitInfo kit, SpriteSystem spriteSystem)
    {
        var accentColor = Color.FromHex("#d5ddd7");
        var accentDim = Color.FromHex("#98a39b");
        var textColor = Color.FromHex("#eef3ef");

        StyleBoxTexture MakeTexture(SpriteSpecifier sprite, string color, bool tile = false)
        {
            var texture = new StyleBoxTexture
            {
                Texture = spriteSystem.Frame0(sprite),
                Modulate = Color.FromHex(color),
                Mode = tile ? StyleBoxTexture.StretchMode.Tile : StyleBoxTexture.StretchMode.Stretch
            };

            if (!tile)
                texture.SetPatchMargin(StyleBox.Margin.All, 2);

            return texture;
        }

        var frameTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/transparent_window_background_bordered.png")), "#a1aea6");
        var headerTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/window_header.png")), "#5f6c63");
        var sectionTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/black_panel_light_thin_border.png")), "#b3bdb6");
        var insetTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/light_panel_background_bordered.png")), "#303833");
        var stripeTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/stripeback.svg.96dpi.png")), "#f2fff6", true);
        var stripeTextureDim = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/stripeback.svg.96dpi.png")), "#d4e2da", true);

        var outerPanel = new PanelContainer
        {
            Margin = new Thickness(4),
            MinWidth = 204,
            MaxWidth = 204,
            MinHeight = 328,
            MaxHeight = 328,
            PanelOverride = frameTexture
        };

        var bodyPanel = new PanelContainer
        {
            Margin = new Thickness(3),
            HorizontalExpand = true,
            VerticalExpand = true,
            PanelOverride = stripeTexture
        };

        var column = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(6)
        };

        var headerPanel = new PanelContainer
        {
            HorizontalExpand = true,
            Margin = new Thickness(0, 0, 0, 6),
            MinHeight = 36,
            PanelOverride = headerTexture
        };

        var headerColumn = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            Margin = new Thickness(6, 5, 6, 4)
        };

        var name = new Label
        {
            Text = kit.Name,
            HorizontalAlignment = HAlignment.Left,
            FontColorOverride = textColor,
            StyleClasses = { "StatusFieldTitle" }
        };

        var headerAccent = new PanelContainer
        {
            Margin = new Thickness(0, 3, 0, 0),
            MinHeight = 2,
            MaxHeight = 2,
            PanelOverride = new StyleBoxFlat { BackgroundColor = accentColor }
        };

        var iconSection = new PanelContainer
        {
            HorizontalExpand = true,
            Margin = new Thickness(0, 0, 0, 6),
            PanelOverride = sectionTexture
        };

        var iconInset = new PanelContainer
        {
            Margin = new Thickness(3),
            HorizontalExpand = true,
            PanelOverride = stripeTextureDim
        };

        var iconColumn = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            Margin = new Thickness(4)
        };

        var iconInner = new PanelContainer
        {
            HorizontalAlignment = HAlignment.Center,
            MinWidth = 48,
            MaxWidth = 48,
            MinHeight = 48,
            MaxHeight = 48,
            PanelOverride = insetTexture
        };

        var icon = new TextureRect
        {
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            SetSize = new System.Numerics.Vector2(64, 64),
            Margin = new Thickness(6),
            Texture = spriteSystem.Frame0(kit.Sprite)
        };

        var descSection = new PanelContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(0, 0, 0, 6),
            PanelOverride = sectionTexture
        };

        var descInset = new PanelContainer
        {
            Margin = new Thickness(3),
            HorizontalExpand = true,
            VerticalExpand = true,
            PanelOverride = stripeTextureDim
        };

        var descColumn = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(5, 5, 4, 5)
        };

        var descTopLine = new PanelContainer
        {
            Margin = new Thickness(0, 0, 8, 4),
            MinHeight = 1,
            MaxHeight = 1,
            PanelOverride = new StyleBoxFlat { BackgroundColor = accentDim }
        };

        var descScroll = new ScrollContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HScrollEnabled = false,
            VScrollEnabled = true,
            ReserveScrollbarSpace = true
        };

        var desc = new RichTextLabel
        {
            HorizontalExpand = true,
            Margin = new Thickness(0, 0, 8, 0),
            ModulateSelfOverride = textColor
        };

        var footerLine = new PanelContainer
        {
            Margin = new Thickness(0, 0, 0, 5),
            MinHeight = 1,
            MaxHeight = 1,
            PanelOverride = new StyleBoxFlat { BackgroundColor = accentDim }
        };

        SelectButton = new Button
        {
            Text = Loc.GetString("slasher-kit-select-button"),
            HorizontalExpand = true,
            HorizontalAlignment = HAlignment.Stretch,
            StyleClasses = { "OpenBoth" }
        };

        headerColumn.AddChild(name);
        headerColumn.AddChild(headerAccent);
        headerPanel.AddChild(headerColumn);

        iconInner.AddChild(icon);
        iconColumn.AddChild(iconInner);
        iconInset.AddChild(iconColumn);
        iconSection.AddChild(iconInset);

        desc.SetMessage(kit.Description);
        descScroll.AddChild(desc);
        descColumn.AddChild(descTopLine);
        descColumn.AddChild(descScroll);
        descInset.AddChild(descColumn);
        descSection.AddChild(descInset);

        column.AddChild(headerPanel);
        column.AddChild(iconSection);
        column.AddChild(descSection);
        column.AddChild(footerLine);
        column.AddChild(SelectButton);
        bodyPanel.AddChild(column);
        outerPanel.AddChild(bodyPanel);
        AddChild(outerPanel);
    }
}
