using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.Slasher.UI;
using Robust.Client.Audio;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Slasher.UI;

/// <summary>
/// Horizontal kit selection window for the Slasher.
/// </summary>
public sealed class SlasherKitSelectMenu : FancyWindow
{
    [Dependency] private readonly EntityManager _entManager = default!;

    private readonly SpriteSystem _spriteSystem = default!;
    private readonly AudioSystem _audioSystem = default!;

    private const float CardSlotHeight = 336f;
    private const float ConnectorHeight = 34f;
    private const float ScrollBarThickness = 10f;

    private readonly BoxContainer _kitsContainer;
    private readonly TierScrollContainer _scroll;
    private readonly Button _normalTab;
    private readonly Button _prestigeTab;
    private readonly List<SlasherKitCard> _cards = new();

    private bool _tierPicked;

    public event Action<int>? OnKitSelected;

    public SlasherKitSelectMenu()
    {
        IoCManager.InjectDependencies(this);
        _spriteSystem = _entManager.System<SpriteSystem>();
        _audioSystem = _entManager.System<AudioSystem>();

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
        Resizable = false;
        SetSize = new System.Numerics.Vector2(720, 570);
        MinSize = new System.Numerics.Vector2(720, 570);

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

        var tabGroup = new ButtonGroup();

        _normalTab = new Button
        {
            Text = Loc.GetString("slasher-kit-tab-normal"),
            ToggleMode = true,
            Group = tabGroup,
            Pressed = true,
            StyleClasses = { "OpenRight" }
        };
        _normalTab.OnPressed += _ => SetTier(showPrestige: false);

        _prestigeTab = new Button
        {
            Text = Loc.GetString("slasher-kit-tab-prestige"),
            ToggleMode = true,
            Group = tabGroup,
            StyleClasses = { "OpenLeft" }
        };
        _prestigeTab.OnPressed += _ => SetTier(showPrestige: true);

        var tabRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Left,
            Margin = new Thickness(0, 4, 0, 0),
            Children = { _normalTab, _prestigeTab }
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

        _scroll = new TierScrollContainer
        {
            HScrollEnabled = true,
            VScrollEnabled = true,
            VerticalExpand = false,
            HorizontalExpand = true,
            VerticalAlignment = VAlignment.Center,
            MinHeight = CardSlotHeight + ScrollBarThickness,
            MaxHeight = CardSlotHeight + ScrollBarThickness,
            Margin = new Thickness(6, 6, 6, 6)
        };
        var scroll = _scroll;

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
        headerColumn.AddChild(tabRow);
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
        _cards.Clear();

        var normals = new List<(int Index, SlasherKitInfo Kit)>();
        var prestigeByReq = new Dictionary<string, (int Index, SlasherKitInfo Kit)>();
        var leftoverPrestige = new List<(int Index, SlasherKitInfo Kit)>();

        for (var i = 0; i < state.Kits.Count; i++)
        {
            var kit = state.Kits[i];
            if (kit.RequiredAscension == null)
                normals.Add((i, kit));
            else if (!prestigeByReq.ContainsKey(kit.RequiredAscension))
                prestigeByReq[kit.RequiredAscension] = (i, kit);
            else
                leftoverPrestige.Add((i, kit));
        }

        var first = true;

        foreach (var (index, kit) in normals)
        {
            if (!first)
                _kitsContainer.AddChild(MakeSeparator());
            first = false;

            (int Index, SlasherKitInfo Kit)? prestige = null;
            if (kit.AscensionId != null && prestigeByReq.Remove(kit.AscensionId, out var matched))
                prestige = matched;

            _kitsContainer.AddChild(MakeColumn(prestige, (index, kit)));
        }

        foreach (var entry in prestigeByReq.Values)
        {
            if (!first)
                _kitsContainer.AddChild(MakeSeparator());
            first = false;
            _kitsContainer.AddChild(MakeColumn(entry, null));
        }

        foreach (var entry in leftoverPrestige)
        {
            if (!first)
                _kitsContainer.AddChild(MakeSeparator());
            first = false;
            _kitsContainer.AddChild(MakeColumn(entry, null));
        }

        _normalTab.Pressed = true;
        _prestigeTab.Pressed = false;
        _tierPicked = false;
    }

    private BoxContainer MakeColumn((int Index, SlasherKitInfo Kit)? prestige, (int Index, SlasherKitInfo Kit)? normal)
    {
        var column = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical
        };

        if (prestige != null)
            column.AddChild(MakeCard(prestige.Value.Index, prestige.Value.Kit));
        else
            column.AddChild(new Control { MinHeight = CardSlotHeight });

        column.AddChild(prestige != null && normal != null
            ? MakeConnector()
            : new Control { MinHeight = ConnectorHeight });

        if (normal != null)
            column.AddChild(MakeCard(normal.Value.Index, normal.Value.Kit));
        else
            column.AddChild(new Control { MinHeight = CardSlotHeight });

        return column;
    }

    private SlasherKitCard MakeCard(int index, SlasherKitInfo kit)
    {
        var card = new SlasherKitCard(kit, _spriteSystem, _audioSystem);
        card.OnSelectConfirmed += () => OnKitSelected?.Invoke(index);
        card.OnThemePlayStarted += StopOtherThemes;
        _cards.Add(card);
        return card;
    }

    private void SetTier(bool showPrestige)
    {
        _normalTab.Pressed = !showPrestige;
        _prestigeTab.Pressed = showPrestige;
        _tierPicked = true;
        _scroll.VScrollTarget = showPrestige ? 0f : float.MaxValue;

        foreach (var card in _cards)
            card.StopMusic();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!_tierPicked)
            _scroll.VScroll = float.MaxValue;
    }

    private static Control MakeConnector()
    {
        var holder = new Control { MinHeight = ConnectorHeight };

        var glow = new PanelContainer
        {
            MinWidth = 6,
            MaxWidth = 6,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Stretch,
            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#ff2a92") }
        };

        glow.AddChild(new PanelContainer
        {
            MinWidth = 2,
            MaxWidth = 2,
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Stretch,
            PanelOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#fff2fa") }
        });

        holder.AddChild(glow);
        return holder;
    }

    private static BoxContainer MakeSeparator()
    {
        var glowHolder = new Control { MinHeight = CardSlotHeight };

        var separatorGlow = new PanelContainer
        {
            MinWidth = 28,
            MaxWidth = 28,
            MinHeight = 3,
            MaxHeight = 3,
            VerticalAlignment = VAlignment.Center,
            HorizontalAlignment = HAlignment.Center,
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

        glowHolder.AddChild(separatorGlow);

        return new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Children =
            {
                new Control { MinHeight = CardSlotHeight + ConnectorHeight },
                glowHolder
            }
        };
    }

    /// <summary>
    /// Stops every card's theme preview except the one that just started playing.
    /// </summary>
    private void StopOtherThemes(SlasherKitCard active)
    {
        foreach (var card in _cards)
        {
            if (card != active)
                card.StopMusic();
        }
    }
}
