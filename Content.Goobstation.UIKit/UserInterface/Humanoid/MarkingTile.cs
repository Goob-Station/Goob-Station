using System.Numerics;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Content.Shared.Humanoid;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.Humanoid;

public enum MarkingTileTint : byte
{
    None,
    Skin,
    Eyes,
    Marking,
}

public sealed class MarkingTileColors
{
    public Color Skin = Color.White;
    public Color Eyes = Color.White;
    public List<Color> Marking = new();

    public Color Get(MarkingTileTint tint, int layer)
    {
        return tint switch
        {
            MarkingTileTint.Skin => Skin,
            MarkingTileTint.Eyes => Eyes,
            MarkingTileTint.None => Color.White,
            _ => Marking.Count == 0 ? Color.White : Marking[Math.Min(layer, Marking.Count - 1)],
        };
    }
}

public readonly record struct MarkingTileLayer(Texture Texture, MarkingTileTint Tint, int ColorLayer, HumanoidVisualLayers? Slot = null);

public sealed class MarkingTile : ContainerButton
{
    public const float TileWidth = 84;
    public const float WideTileWidth = 112;
    private const float ArtSize = 64;

    private static readonly Color NameColor = Color.FromHex("#9aa1a8");

    private readonly LayeredTextureControl _art;
    private readonly Label _star;

    public string? MarkingId { get; }
    public string DisplayName { get; }

    public event Action<MarkingTile>? RightClicked;

    public MarkingTile(string? markingId, string displayName, MarkingTileColors colors, List<MarkingTileLayer> layers, UIBox2 region, EntityUid? doll, float width = TileWidth)
    {
        MarkingId = markingId;
        DisplayName = displayName;
        ToggleMode = true;
        SetWidth = width;
        StyleClasses.Add(HullStyleClasses.Slot);
        ToolTip = displayName;

        _art = new LayeredTextureControl(colors, layers, region, doll)
        {
            SetSize = new Vector2(ArtSize, ArtSize),
            HorizontalAlignment = HAlignment.Center,
        };
        _star = new Label
        {
            Text = "★",
            StyleClasses = { HullStyleClasses.PostNew },
            HorizontalAlignment = HAlignment.Right,
            VerticalAlignment = VAlignment.Top,
            Visible = false,
        };

        var column = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 2,
        };
        column.AddChild(_art);
        var name = new RichTextLabel
        {
            HorizontalExpand = true,
            Margin = new Thickness(2, 0),
            MouseFilter = MouseFilterMode.Ignore,
        };
        name.SetMessage(FormattedMessage.FromMarkupPermissive($"[font size=7]{FormattedMessage.EscapeText(displayName)}[/font]"), defaultColor: NameColor);
        column.AddChild(name);

        AddChild(column);
        AddChild(_star);

        OnKeyBindDown += args =>
        {
            if (args.Function != EngineKeyFunctions.UIRightClick)
                return;

            RightClicked?.Invoke(this);
            args.Handle();
        };
    }

    public bool Favourite
    {
        get => _star.Visible;
        set => _star.Visible = value;
    }
}
