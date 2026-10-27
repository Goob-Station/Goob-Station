using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Graphics.RSI;

namespace Content.Goobstation.UIKit.UserInterface.Humanoid;

public sealed partial class LayeredTextureControl : Control
{
    [Dependency] private IEntityManager _entMan = default!;

    private const float BaseSize = 32;

    private readonly MarkingTileColors _colors;
    private readonly List<MarkingTileLayer> _layers;
    private readonly UIBox2 _region;
    private readonly EntityUid? _doll;
    private readonly Dictionary<int, List<MarkingTileLayer>> _inserts = new();
    private readonly List<MarkingTileLayer> _trailing = new();

    public LayeredTextureControl(MarkingTileColors colors, List<MarkingTileLayer> layers, UIBox2 region, EntityUid? doll = null)
    {
        IoCManager.InjectDependencies(this);
        _colors = colors;
        _layers = layers;
        _region = region;
        _doll = doll;
        RectClipContent = true;
        MouseFilter = MouseFilterMode.Ignore;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var scale = PixelSize.X / (_region.Width * BaseSize);

        if (_doll is not { } doll || !_entMan.TryGetComponent<SpriteComponent>(doll, out var sprite))
        {
            foreach (var layer in _layers)
            {
                DrawTexture(handle, layer.Texture, _colors.Get(layer.Tint, layer.ColorLayer), scale);
            }

            return;
        }

        var spriteSystem = _entMan.System<SpriteSystem>();
        foreach (var list in _inserts.Values)
        {
            list.Clear();
        }
        _trailing.Clear();

        foreach (var layer in _layers)
        {
            if (layer.Slot is { } slot && spriteSystem.LayerMapTryGet((doll, sprite), slot, out var index, false))
            {
                if (!_inserts.TryGetValue(index, out var list))
                    _inserts[index] = list = new List<MarkingTileLayer>();

                list.Add(layer);
            }
            else
            {
                _trailing.Add(layer);
            }
        }

        var i = 0;
        foreach (var spriteLayer in sprite.AllLayers)
        {
            if (spriteLayer.Visible && GetTexture(spriteLayer) is { } texture)
                DrawTexture(handle, texture, spriteLayer.Color * sprite.Color, scale);

            if (_inserts.TryGetValue(i, out var inserted))
            {
                foreach (var layer in inserted)
                {
                    DrawTexture(handle, layer.Texture, _colors.Get(layer.Tint, layer.ColorLayer), scale);
                }
            }

            i++;
        }

        foreach (var layer in _trailing)
        {
            DrawTexture(handle, layer.Texture, _colors.Get(layer.Tint, layer.ColorLayer), scale);
        }
    }

    private void DrawTexture(DrawingHandleScreen handle, Texture texture, Color color, float scale)
    {
        var size = (Vector2) texture.Size;
        var topLeft = new Vector2(
            (BaseSize - size.X) / 2f - _region.Left * BaseSize,
            (BaseSize - size.Y) / 2f - _region.Top * BaseSize) * scale;

        handle.DrawTextureRect(texture, UIBox2.FromDimensions(topLeft, size * scale), color);
    }

    private static Texture? GetTexture(ISpriteLayer layer)
    {
        if (layer.Texture is { } texture)
            return texture;

        if (layer.ActualRsi is { } rsi && rsi.TryGetState(layer.RsiState, out var state))
            return state.GetFrame(RsiDirection.South, 0);

        return null;
    }
}
