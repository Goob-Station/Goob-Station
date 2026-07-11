using System;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Client._Pirate.UserInterface.Controls;

/// <summary>
/// A reusable collapsible section with framed background and styled heading.
/// Designed for menus that need card-like grouped lists.
/// </summary>
public sealed class FramedCollapsible : PanelContainer
{
    private const string ChevronCollapsedIconPath = "/Textures/Interface/Nano/triangle_right.png";
    private const string ChevronExpandedIconPath = "/Textures/Interface/Nano/inverted_triangle.svg.png";
    private static readonly System.Numerics.Vector2 ChevronSize = new(12, 12);
    private static readonly Color HeadingNormalColor = new(0.17f, 0.17f, 0.20f, 0.95f);

    public event Action<bool>? OnToggled;

    public CollapsibleBody Body { get; }

    public bool Expanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value)
                return;
            _expanded = value;
            ApplyExpandedState();
        }
    }

    public bool IsEmptySection
    {
        get => _isEmptySection;
        set
        {
            if (_isEmptySection == value)
                return;
            _isEmptySection = value;
            ApplyExpandedState();
        }
    }

    private readonly Collapsible _collapsible;
    private readonly CollapsibleHeading _heading;
    private readonly StyleBoxFlat _headingStyleBox;
    private readonly TextureRect? _chevronIcon;
    private bool _expanded;
    private bool _isEmptySection;
    private bool _headingHovered;

    public FramedCollapsible(string title)
    {
        HorizontalExpand = true;
        PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = new Color(0.11f, 0.11f, 0.13f, 0.92f),
            ContentMarginLeftOverride = 5,
            ContentMarginRightOverride = 5,
            ContentMarginTopOverride = 5,
            ContentMarginBottomOverride = 5,
        };

        _headingStyleBox = new StyleBoxFlat
        {
            BackgroundColor = HeadingNormalColor,
            ContentMarginLeftOverride = 5,
            ContentMarginRightOverride = 5,
            ContentMarginTopOverride = 4,
            ContentMarginBottomOverride = 4,
        };
        _heading = new CollapsibleHeading(title)
        {
            HorizontalExpand = true,
            StyleBoxOverride = _headingStyleBox,
            ModulateSelfOverride = Color.White,
        };
        _heading.Label.HorizontalExpand = true;
        _heading.Label.ClipText = true;
        _heading.OnMouseEntered += _ =>
        {
            _headingHovered = true;
            UpdateHeadingModulate();
        };
        _heading.OnMouseExited += _ =>
        {
            _headingHovered = false;
            UpdateHeadingModulate();
        };
        _chevronIcon = FindChevronIcon(_heading);
        if (_chevronIcon != null)
        {
            _chevronIcon.MinSize = ChevronSize;
            _chevronIcon.SetSize = ChevronSize;
            _chevronIcon.Stretch = TextureRect.StretchMode.KeepCentered;
        }

        Body = new CollapsibleBody
        {
            Margin = new Thickness(0, 5, 0, 0),
        };

        _collapsible = new Collapsible(_heading, Body)
        {
            HorizontalExpand = true,
        };
        AddChild(_collapsible);

        _heading.OnToggled += args =>
        {
            _expanded = args.Pressed;
            ApplyExpandedState();
            OnToggled?.Invoke(Expanded);
        };

        ApplyExpandedState();
    }

    private void ApplyExpandedState()
    {
        // Important ordering dependency:
        // `_collapsible.BodyVisible` drives CollapsibleHeading pressed state (chevron direction) and also applies
        // Collapsible's internal OnToggled/body visibility behavior. We set it first to keep heading state synced.
        // We then explicitly override `Body.Visible` for empty sections so the body stays hidden even when expanded.
        // Final body visibility depends on this call order; do not assume Collapsible internal handler ordering is stable.
        _collapsible.BodyVisible = _expanded;

        if (_chevronIcon != null)
            _chevronIcon.TexturePath = _expanded ? ChevronExpandedIconPath : ChevronCollapsedIconPath;

        // Explicit final visibility override after _collapsible state sync.
        Body.Visible = !_isEmptySection && _expanded;
        UpdateHeadingModulate();
    }

    private void UpdateHeadingModulate()
    {
        // Keep the global button palette out of this custom-colored heading. Hover is expressed through the
        // owned background directly so no stylesheet modulation can darken it again.
        _heading.ModulateSelfOverride = Color.White;
        _headingStyleBox.BackgroundColor = _headingHovered
            ? LightenForHover(HeadingNormalColor)
            : HeadingNormalColor;
    }

    private static Color LightenForHover(Color color, float factor = 1.2f)
    {
        return new Color(
            Math.Min(1f, color.R * factor),
            Math.Min(1f, color.G * factor),
            Math.Min(1f, color.B * factor),
            color.A);
    }

    private static TextureRect? FindChevronIcon(Control control)
    {
        foreach (var child in control.Children)
        {
            if (child is TextureRect textureRect && IsChevronTextureRect(textureRect))
                return textureRect;

            var nested = FindChevronIcon(child);
            if (nested != null)
                return nested;
        }

        return null;
    }

    private static bool IsChevronTextureRect(TextureRect textureRect)
    {
        if (textureRect.Name == "Chevron")
            return true;

        return textureRect.StyleClasses.Contains(OptionButton.StyleClassOptionTriangle);
    }
}
