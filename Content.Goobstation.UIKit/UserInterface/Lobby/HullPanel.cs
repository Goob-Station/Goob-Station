using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class HullPanel : PanelContainer
{
    public static readonly Color PlateColor = Color.FromHex("#2c3035");
    public static readonly Color EdgeColor = Color.FromHex("#15171a");
    public static readonly Color HighlightColor = Color.FromHex("#4d535a");
    public static readonly Color ShadowColor = Color.FromHex("#1d2024");

    private const string ScrewTexturePath = "/Textures/_Goobstation/Interface/Lobby/screw_diagonal.png";
    private const float ScrewInset = 3;

    private Texture? _screwTexture;

    public bool Screws { get; set; } = true;

    public HullPanel()
    {
        PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = PlateColor,
            BorderColor = EdgeColor,
            BorderThickness = new Thickness(1),
        };

        if (IoCManager.Resolve<IResourceCache>().TryGetResource<TextureResource>(ScrewTexturePath, out var screw))
            _screwTexture = screw.Texture;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var size = PixelSize;
        var inner = UIBox2.FromDimensions(new Vector2(1, 1), new Vector2(size.X - 2, size.Y - 2));

        handle.DrawLine(inner.TopLeft, new Vector2(inner.Right, inner.Top), HighlightColor);
        handle.DrawLine(inner.TopLeft, new Vector2(inner.Left, inner.Bottom), HighlightColor);
        handle.DrawLine(new Vector2(inner.Left, inner.Bottom), inner.BottomRight, ShadowColor);
        handle.DrawLine(new Vector2(inner.Right, inner.Top), inner.BottomRight, ShadowColor);

        if (!Screws || _screwTexture is not { } screw)
            return;

        var screwSize = (Vector2) screw.Size * UIScale;
        var near = ScrewInset * UIScale;
        var farX = size.X - near - screwSize.X;
        var farY = size.Y - near - screwSize.Y;

        handle.DrawTextureRect(screw, UIBox2.FromDimensions(new Vector2(near, near), screwSize));
        handle.DrawTextureRect(screw, UIBox2.FromDimensions(new Vector2(farX, near), screwSize));
        handle.DrawTextureRect(screw, UIBox2.FromDimensions(new Vector2(near, farY), screwSize));
        handle.DrawTextureRect(screw, UIBox2.FromDimensions(new Vector2(farX, farY), screwSize));
    }
}
