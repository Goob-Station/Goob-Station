using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public partial class TintButton : Button
{
    [Dependency] private IResourceCache _cache = default!;

    private const string TexturePath = "/Textures/_Goobstation/Interface/Lobby";
    private const float PatchMargin = 9;

    private Color? _tint;
    private Color _textColor = Color.White;
    private bool _applied;

    public TintButton()
    {
        IoCManager.InjectDependencies(this);
    }

    public Color? Tint
    {
        get => _tint;
        set
        {
            _tint = value;
            UpdateTint();
        }
    }

    public Color TextColor
    {
        get => _textColor;
        set
        {
            _textColor = value;
            UpdateTint();
        }
    }

    public void SetTint(Color? tint, Color textColor)
    {
        _textColor = textColor;
        Tint = tint;
    }

    protected override void DrawModeChanged()
    {
        base.DrawModeChanged();
        UpdateTint();
    }

    private void UpdateTint()
    {
        if (_tint is not { } tint)
        {
            if (!_applied)
                return;

            _applied = false;
            StyleBoxOverride = null;
            Label.FontColorOverride = null;
            return;
        }

        _applied = true;

        var (state, top, bottom) = DrawMode switch
        {
            DrawModeEnum.Hover => ("_hover", 3f, 6f),
            DrawModeEnum.Pressed => ("_pressed", 5f, 4f),
            DrawModeEnum.Disabled => ("_disabled", 3f, 6f),
            _ => (string.Empty, 3f, 6f),
        };

        var box = new StyleBoxTexture
        {
            Texture = _cache.GetResource<TextureResource>($"{TexturePath}/button_tint{state}.png").Texture,
            Modulate = tint,
        };
        box.SetPatchMargin(StyleBox.Margin.All, PatchMargin);
        box.SetContentMarginOverride(StyleBox.Margin.Horizontal, 8);
        box.SetContentMarginOverride(StyleBox.Margin.Top, top);
        box.SetContentMarginOverride(StyleBox.Margin.Bottom, bottom);
        StyleBoxOverride = box;

        Label.FontColorOverride = _textColor;
    }
}
