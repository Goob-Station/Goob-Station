using Content.Client.UserInterface.Systems.Guidebook;
using Content.Goobstation.Shared.Slasher.UI;
using Content.Shared.Guidebook;
using Robust.Client.Audio;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.IoC;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Slasher.UI;

/// <summary>
/// A kit card for the Slasher kit selection menu.
/// </summary>
public sealed class SlasherKitCard : Control
{
    private const float ConfirmTimeout = 3f;
    public readonly Button SelectButton;
    private readonly TextureButton _playMusicButton;
    private readonly AudioSystem _audioSystem;
    private readonly SoundSpecifier? _themeSong;
    private readonly Texture _playTexture;
    private readonly Texture _stopTexture;
    private (EntityUid Entity, AudioComponent Component)? _themeStream;
    private bool _confirming;
    private float _confirmTimer;
    public event Action? OnSelectConfirmed;
    public event Action<SlasherKitCard>? OnThemePlayStarted;

    public SlasherKitCard(SlasherKitInfo kit, SpriteSystem spriteSystem, AudioSystem audioSystem)
    {
        _audioSystem = audioSystem;
        _themeSong = kit.ThemeSong;
        _playTexture = spriteSystem.Frame0(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/AdminActions/play.png")));
        _stopTexture = spriteSystem.Frame0(new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/AdminActions/pause.png")));

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
            HorizontalExpand = true,
            FontColorOverride = textColor,
            StyleClasses = { "StatusFieldTitle" }
        };

        var headerRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
        };
        headerRow.AddChild(name);

        if (kit.Guide is { } guideId)
        {
            var guideButton = new Button
            {
                Text = "?",
                ToolTip = Loc.GetString("slasher-kit-guide-button"),
                MinSize = new System.Numerics.Vector2(24, 24),
                VerticalAlignment = VAlignment.Center,
                StyleBoxOverride = insetTexture,
            };
            guideButton.Label.HorizontalAlignment = HAlignment.Center;
            guideButton.Label.FontColorOverride = textColor;
            guideButton.OnPressed += _ => OpenGuide(guideId);
            headerRow.AddChild(guideButton);
        }

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
        SelectButton.OnPressed += OnSelectPressed;

        if (!kit.Unlocked)
        {
            SelectButton.Disabled = true;
            SelectButton.Text = Loc.GetString("slasher-kit-locked-button");
        }

        _playMusicButton = new TextureButton
        {
            TextureNormal = _playTexture,
            Scale = new System.Numerics.Vector2(0.75f, 0.75f),
            VerticalAlignment = VAlignment.Center,
            HorizontalAlignment = HAlignment.Center,
            Disabled = _themeSong == null,
            ToolTip = Loc.GetString("slasher-kit-play-music-button"),
            ModulateSelfOverride = _themeSong == null ? accentDim : Color.White
        };
        _playMusicButton.OnPressed += _ => ToggleThemeSong();

        var actionRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            SeparationOverride = 4
        };

        headerColumn.AddChild(headerRow);
        headerColumn.AddChild(headerAccent);
        headerPanel.AddChild(headerColumn);

        iconInner.AddChild(icon);
        iconColumn.AddChild(iconInner);
        iconInset.AddChild(iconColumn);
        iconSection.AddChild(iconInset);

        var description = new FormattedMessage();
        if (!kit.Unlocked && kit.RequiredAscension != null)
        {
            description.PushColor(Color.FromHex("#ff4d6d"));
            description.AddText(Loc.GetString("slasher-kit-locked-requirement", ("required", kit.RequiredAscension)));
            description.Pop();
            description.PushNewline();
            description.PushNewline();
        }
        description.AddText(kit.Description);
        desc.SetMessage(description);
        descScroll.AddChild(desc);
        descColumn.AddChild(descTopLine);
        descColumn.AddChild(descScroll);
        descInset.AddChild(descColumn);
        descSection.AddChild(descInset);

        column.AddChild(headerPanel);
        column.AddChild(iconSection);
        column.AddChild(descSection);
        column.AddChild(footerLine);
        actionRow.AddChild(SelectButton);
        actionRow.AddChild(_playMusicButton);
        column.AddChild(actionRow);
        bodyPanel.AddChild(column);
        outerPanel.AddChild(bodyPanel);
        AddChild(outerPanel);
    }

    private static void OpenGuide(string guideId)
    {
        IoCManager.Resolve<IUserInterfaceManager>()
            .GetUIController<GuidebookUIController>()
            .OpenGuidebook(selected: new ProtoId<GuideEntryPrototype>(guideId));
    }

    private void OnSelectPressed(BaseButton.ButtonEventArgs args)
    {
        if (!_confirming)
        {
            _confirming = true;
            _confirmTimer = ConfirmTimeout;
            SelectButton.Text = Loc.GetString("slasher-kit-select-confirm-button");
            SelectButton.ModulateSelfOverride = Color.FromHex("#ff4d6d");
            return;
        }

        OnSelectConfirmed?.Invoke();
    }

    private void ResetConfirm()
    {
        _confirming = false;
        SelectButton.Text = Loc.GetString("slasher-kit-select-button");
        SelectButton.ModulateSelfOverride = null;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!_confirming)
            return;

        _confirmTimer -= args.DeltaSeconds;

        if (_confirmTimer <= 0f)
            ResetConfirm();
    }

    private void ToggleThemeSong()
    {
        if (_themeStream != null)
        {
            StopMusic();
            return;
        }

        if (_themeSong == null)
            return;

        var stream = _audioSystem.PlayGlobal(_themeSong, Filter.Local(), false);

        if (stream == null)
            return;

        _themeStream = (stream.Value.Entity, stream.Value.Component);
        _playMusicButton.TextureNormal = _stopTexture;
        _playMusicButton.ToolTip = Loc.GetString("slasher-kit-stop-music-button");

        OnThemePlayStarted?.Invoke(this);
    }

    public void StopMusic()
    {
        if (_themeStream != null)
            _audioSystem.Stop(_themeStream.Value.Entity, _themeStream.Value.Component);

        _themeStream = null;
        _playMusicButton.TextureNormal = _playTexture;
        _playMusicButton.ToolTip = Loc.GetString("slasher-kit-play-music-button");
    }

    protected override void ExitedTree()
    {
        base.ExitedTree();

        StopMusic();
    }
}
