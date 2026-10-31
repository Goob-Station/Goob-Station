using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Controls.NodeTree;

public enum NodeTreeState : byte
{
    Unlocked,
    Available,
    Unaffordable,
    Locked,
}

public sealed class NodeTreeButton : ContainerButton
{
    public static readonly Color SelectedBorderColor = Color.White;

    private readonly StyleBoxFlat _box = new() { BorderThickness = new Thickness(2) };

    public readonly TextureRect Icon;

    public NodeTreeState State
    {
        get => _state;
        set
        {
            _state = value;
            UpdateColors();
        }
    }

    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            UpdateColors();
        }
    }

    private NodeTreeState _state = NodeTreeState.Locked;
    private bool _selected;

    public NodeTreeButton()
    {
        StyleBoxOverride = _box;
        Icon = new TextureRect
        {
            Stretch = TextureRect.StretchMode.KeepAspectCentered,
            CanShrink = true,
            Margin = new Thickness(8),
        };
        AddChild(Icon);
        UpdateColors();
    }

    public static (Color Background, Color Hovered, Color Border) GetColors(NodeTreeState state)
    {
        return state switch
        {
            NodeTreeState.Unlocked => (Color.DarkOliveGreen, Color.PaleGreen, Color.LimeGreen),
            NodeTreeState.Available => (Color.FromHex("#7c7d2a"), Color.FromHex("#ecfa52"), Color.FromHex("#e8fa25")),
            NodeTreeState.Unaffordable => (Color.FromHex("#6b572f"), Color.FromHex("#fad398"), Color.FromHex("#cca031")),
            _ => (Color.DarkRed, Color.PaleVioletRed, Color.Crimson),
        };
    }

    protected override void DrawModeChanged()
    {
        base.DrawModeChanged();
        UpdateColors();
    }

    private void UpdateColors()
    {
        var (background, hovered, border) = GetColors(_state);
        _box.BackgroundColor = DrawMode is DrawModeEnum.Hover or DrawModeEnum.Pressed ? hovered : background;
        _box.BorderColor = _selected ? SelectedBorderColor : border;
    }
}
