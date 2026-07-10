using System.Numerics;
using Content.Client._Pirate.Loadouts;
using Content.Shared.Clothing;
using Content.Shared.Preferences.Loadouts;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client._Pirate.Lobby.UI.Loadouts;

public sealed class LoadoutIconButton : Button
{
    private const int MaxCaptionNameLength = 13;
    private const int TrimmedCaptionLength = 12;

    [Dependency] private readonly IEntityManager _entManager = default!;

    public event Action<string, string>? OnCustomizePressed;

    private string _defaultName = string.Empty;
    private string _defaultDescription = string.Empty;

    private static readonly StyleBoxFlat NormalStyle = CreateStyle("#2a2a35", "#32323e");
    private static readonly StyleBoxFlat HoverStyle = CreateStyle("#2a3a4a", "#32323e");
    private static readonly StyleBoxFlat SelectedStyle = CreateStyle("#2a3a4a", "#60a5fa");
    private static readonly StyleBoxFlat DisabledStyle = CreateStyle("#1a1a22", "#2a2a2a");
    private static readonly StyleBoxFlat FlashStyle = CreateStyle("#3a3a2a", "#fbbf24", 2);

    private readonly Label _caption;
    private readonly TextureRect _lockOverlay;
    private bool _flashing;
    private int _flashGeneration;
    private readonly bool _supportsColor;

    private readonly EntityUid? _entity;

    public LoadoutIconButton(LoadoutPrototype loadout, string name, string? customColorTint = null, FormattedMessage? reason = null)
    {
        IoCManager.InjectDependencies(this);

        _supportsColor = loadout.CustomColorTint;
        ToggleMode = true;
        // Extra height for the caption.
        MinSize = new Vector2(108, 132);
        SetSize = new Vector2(108, 132);
        StyleBoxOverride = NormalStyle;
        ModulateSelfOverride = Color.White;

        // Keep sprites inside the 96px icon area.
        var sprite = new SpriteView
        {
            Scale = new Vector2(3f, 3f),
            OverrideDirection = Direction.South,
            VerticalAlignment = VAlignment.Center,
            HorizontalAlignment = HAlignment.Center,
            SetSize = new Vector2(96, 96),
        };

        var entityProto = ResolveDisplayEntity(loadout);
        var displayName = name;
        var description = string.Empty;

        if (entityProto != null)
        {
            _entity = _entManager.SpawnEntity(entityProto, MapCoordinates.Nullspace);
            SetCustomColor(customColorTint);
            sprite.SetEntity(_entity);

            if (_entManager.TryGetComponent(_entity.Value, out MetaDataComponent? meta))
            {
                displayName = meta.EntityName;
                description = meta.EntityDescription;
            }
        }

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = loadout.ID;

        _defaultName = displayName;
        _defaultDescription = description;

        // Clip sprites so they cannot cover the caption.
        var spriteRegion = new Control { MinSize = new Vector2(0, 96), RectClipContent = true };
        spriteRegion.AddChild(sprite);

        // Full name remains in the tooltip.
        var captionText = displayName.Length > MaxCaptionNameLength
            ? string.Concat(displayName.AsSpan(0, TrimmedCaptionLength), "...")
            : displayName;

        // ClipText labels must stay stretched.
        _caption = new Label
        {
            Text = captionText,
            ClipText = true,
            Align = Label.AlignMode.Center,
            HorizontalExpand = true,
            MinSize = new Vector2(0, 26),
            StyleClasses = { "font-small" },
        };

        AddChild(new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
            Children = { spriteRegion, _caption },
        });

        // Shown only while disabled.
        _lockOverlay = new TextureRect
        {
            TexturePath = "/Textures/Interface/Nano/lock.svg.192dpi.png",
            SetSize = new Vector2(16, 16),
            HorizontalAlignment = HAlignment.Right,
            VerticalAlignment = VAlignment.Top,
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            Visible = false,
        };
        AddChild(_lockOverlay);

        TooltipSupplier = _ =>
        {
            var tooltip = new Tooltip();
            var text = string.IsNullOrWhiteSpace(description)
                ? displayName
                : $"{displayName}\n\n{description}";

            if (reason != null)
                text += $"\n\n{reason}";

            tooltip.SetMessage(FormattedMessage.FromUnformatted(text));
            return tooltip;
        };

        AddCustomizeButton();
    }

    private EntProtoId? ResolveDisplayEntity(LoadoutPrototype loadout)
    {
        var entity = loadout.DummyEntity ?? _entManager.System<LoadoutSystem>().GetFirstOrNull(loadout);
        if (entity != null)
            return entity;

        foreach (var equipment in loadout.Equipment.Values)
            return equipment;

        if (loadout.Inhand.Count != 0)
            return loadout.Inhand[0];

        foreach (var storage in loadout.Storage.Values)
        {
            if (storage.Count != 0)
                return storage[0];
        }

        return null;
    }

    public void SetCustomColor(string? customColorTint)
    {
        if (_entity == null || string.IsNullOrEmpty(customColorTint))
            return;

        _entManager.System<LoadoutTintSystem>().SetTint(_entity.Value, Color.FromHex(customColorTint));
    }

    /// <summary>Briefly highlights the button.</summary>
    public void Flash()
    {
        var generation = ++_flashGeneration;
        _flashing = true;
        ApplyStyle();

        Timer.Spawn(TimeSpan.FromSeconds(1.5), () =>
        {
            if (Disposed || generation != _flashGeneration)
                return;

            _flashing = false;
            ApplyStyle();
        });
    }

    private void AddCustomizeButton()
    {
        // Top-left button opens the customize dialog.
        var button = new ContainerButton
        {
            StyleBoxOverride = new StyleBoxEmpty(),
            MinSize = new Vector2(24, 24),
            SetSize = new Vector2(24, 24),
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Top,
            ToolTip = Loc.GetString("loadout-customize-tooltip"),
        };

        // Palette for colorable items, gear otherwise.
        var iconPath = _supportsColor
            ? "/Textures/_Pirate/Interface/VerbIcons/palette.svg.192dpi.png"
            : "/Textures/Interface/Nano/gear.svg.192dpi.png";

        button.AddChild(new TextureRect
        {
            TexturePath = iconPath,
            SetSize = new Vector2(16, 16),
            VerticalAlignment = VAlignment.Center,
            HorizontalAlignment = HAlignment.Center,
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
        });

        button.OnPressed += _ => OnCustomizePressed?.Invoke(_defaultName, _defaultDescription);
        AddChild(button);
    }

    protected override void DrawModeChanged()
    {
        base.DrawModeChanged();
        ApplyStyle();
    }

    private void ApplyStyle()
    {
        StyleBoxOverride = _flashing
            ? FlashStyle
            : DrawMode switch
            {
                DrawModeEnum.Disabled => DisabledStyle,
                DrawModeEnum.Pressed => SelectedStyle,
                DrawModeEnum.Hover => HoverStyle,
                _ => NormalStyle,
            };

        var disabled = DrawMode == DrawModeEnum.Disabled;
        ModulateSelfOverride = disabled ? new Color(1f, 1f, 1f, 0.55f) : Color.White;

        if (_lockOverlay != null)
            _lockOverlay.Visible = disabled;

        if (_caption != null)
            _caption.FontColorOverride = disabled ? new Color(0.6f, 0.6f, 0.65f) : null;
    }

    private static StyleBoxFlat CreateStyle(string backgroundColor, string borderColor, float borderThickness = 1)
    {
        return new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex(backgroundColor),
            BorderColor = Color.FromHex(borderColor),
            BorderThickness = new Thickness(borderThickness),
            ContentMarginLeftOverride = 4,
            ContentMarginRightOverride = 4,
            ContentMarginTopOverride = 4,
            ContentMarginBottomOverride = 4,
        };
    }

    [Obsolete("Controls should only be removed from UI tree instead of being disposed")]
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing || _entity == null)
            return;

        _entManager.DeleteEntity(_entity);
    }
}
