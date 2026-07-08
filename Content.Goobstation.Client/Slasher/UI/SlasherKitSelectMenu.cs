using Content.Client.IoC;
using Content.Client.Resources;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.Slasher.UI;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Slasher.UI;

/// <summary>
/// Horizontal kit selection window for the Slasher.
/// </summary>
public sealed class SlasherKitSelectMenu : FancyWindow
{
    [Dependency] private readonly EntityManager _entManager = default!;

    private readonly SpriteSystem _spriteSystem = default!;

    private readonly BoxContainer _kitsContainer;

    public event Action<int>? OnKitSelected;

    public SlasherKitSelectMenu()
    {
        IoCManager.InjectDependencies(this);
        _spriteSystem = _entManager.System<SpriteSystem>();

        var textColor = Color.FromHex("#fff5f8");

        StyleBoxTexture MakeTexture(SpriteSpecifier sprite, string color, bool tile = false)
        {
            var texture = new StyleBoxTexture
            {
                Texture = _spriteSystem.Frame0(sprite),
                Modulate = Color.FromHex(color),
                Mode = tile ? StyleBoxTexture.StretchMode.Tile : StyleBoxTexture.StretchMode.Stretch
            };

            if (!tile)
                texture.SetPatchMargin(StyleBox.Margin.All, 2);

            return texture;
        }

        var outerChromeTexture = MakeTexture(new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Heretic/Effects/effects.rsi"), "cloud_swirl"), "#ff1744", true);
        var frameTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/transparent_window_background_bordered.png")), "#ff9caf");
        var rootTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/window_background_bordered.png")), "#1a0c10");
        var headerTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/window_header_alert.png")), "#d61f49");
        var panelTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/black_panel_red_thin_border.png")), "#ffb0be");
        var insetTexture = MakeTexture(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Nano/light_panel_background_bordered.png")), "#4b1e29");

        Title = Loc.GetString("slasher-kit-select-title");
        HideCloseButton(this);
        MinSize = new System.Numerics.Vector2(720, 525);

        var chromeFrame = new PanelContainer
        {
            Margin = new Thickness(4, 30, 4, 4),
            HorizontalExpand = true,
            VerticalExpand = true,
            PanelOverride = outerChromeTexture
        };

        var chromeInset = new PanelContainer
        {
            Margin = new Thickness(12),
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        var frame = new PanelContainer
        {
            Margin = new Thickness(3),
            HorizontalExpand = true,
            VerticalExpand = true,
            PanelOverride = frameTexture
        };

        var rootPanel = new PanelContainer
        {
            Margin = new Thickness(3),
            HorizontalExpand = true,
            VerticalExpand = true,
            PanelOverride = rootTexture
        };

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Margin = new Thickness(8, 18, 8, 8)
        };

        var headerPanel = new PanelContainer
        {
            HorizontalExpand = true,
            Margin = new Thickness(0, 0, 0, 8),
            MinHeight = 42,
            PanelOverride = headerTexture
        };

        var headerColumn = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            Margin = new Thickness(8, 6, 8, 5)
        };

        var header = new Label
        {
            Text = Loc.GetString("slasher-kit-select-header"),
            HorizontalAlignment = HAlignment.Left,
            FontColorOverride = textColor,
            StyleClasses = { "StatusFieldTitle" }
        };

        var scrollFrame = new PanelContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            PanelOverride = panelTexture
        };

        var scrollInset = new PanelContainer
        {
            Margin = new Thickness(3),
            HorizontalExpand = true,
            VerticalExpand = true,
            PanelOverride = insetTexture
        };

        var scroll = new ScrollContainer
        {
            HScrollEnabled = true,
            VScrollEnabled = false,
            VerticalExpand = true,
            HorizontalExpand = true,
            Margin = new Thickness(6, 6, 6, 6)
        };

        var staticSprite = new AnimatedTextureRect
        {
            HorizontalExpand = true,
            VerticalExpand = true
        };
        staticSprite.SetFromSpriteSpecifier(new SpriteSpecifier.Rsi(new("_Goobstation/Interface/rnd-static.rsi"), "static"));
        staticSprite.DisplayRect.CanShrink = true;
        staticSprite.DisplayRect.Stretch = TextureRect.StretchMode.Scale;

        _kitsContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            VerticalExpand = true,
            SeparationOverride = 6,
            Margin = new Thickness(2)
        };

        headerColumn.AddChild(header);
        headerPanel.AddChild(headerColumn);
        scroll.AddChild(_kitsContainer);
        staticSprite.AddChild(scroll);
        scrollInset.AddChild(staticSprite);
        scrollFrame.AddChild(scrollInset);
        root.AddChild(headerPanel);
        root.AddChild(scrollFrame);
        rootPanel.AddChild(root);
        frame.AddChild(rootPanel);
        chromeInset.AddChild(frame);
        chromeFrame.AddChild(chromeInset);
        AddChild(chromeFrame);
    }

    private static void HideCloseButton(Control control)
    {
        foreach (var child in control.Children)
        {
            if (child is TextureButton { Name: "CloseButton" } closeButton)
            {
                closeButton.Visible = false;
                return;
            }

            HideCloseButton(child);
        }
    }

    public override void Close() { }
    public void ForceClose() => base.Close();

    public void UpdateState(SlasherKitSelectBoundUserInterfaceState state)
    {
        _kitsContainer.DisposeAllChildren();

        for (var i = 0; i < state.Kits.Count; i++)
        {
            if (i > 0)
            {
                var separatorGlow = new PanelContainer
                {
                    MinWidth = 28,
                    MaxWidth = 28,
                    MinHeight = 3,
                    MaxHeight = 3,
                    VerticalAlignment = VAlignment.Center,
                    HorizontalExpand = false,
                    Margin = new Thickness(4, 0),
                    PanelOverride = new StyleBoxFlat
                    {
                        BackgroundColor = Color.FromHex("#ff2a92")
                    }
                };

                separatorGlow.AddChild(new PanelContainer
                {
                    MinWidth = 20,
                    MaxWidth = 20,
                    MinHeight = 1,
                    MaxHeight = 1,
                    VerticalAlignment = VAlignment.Center,
                    HorizontalAlignment = HAlignment.Center,
                    PanelOverride = new StyleBoxFlat
                    {
                        BackgroundColor = Color.FromHex("#fff2fa")
                    }
                });

                _kitsContainer.AddChild(separatorGlow);
            }

            var index = i;
            var kit = state.Kits[i];
            var card = new SlasherKitCard(kit, _spriteSystem);
            card.SelectButton.OnPressed += _ => OnKitSelected?.Invoke(index);
            _kitsContainer.AddChild(card);
        }
    }
}
