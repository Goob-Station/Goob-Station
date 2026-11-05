using System.Numerics;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.Humanoid;

public sealed partial class LayeredTextureControl : Control
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MarkingManager _markingManager = default!;

    private const float BaseSize = 32;
    private const string StencilClear = "StencilClear";

    private readonly MarkingTileColors _colors;
    private readonly List<MarkingTileLayer> _layers;
    private readonly UIBox2 _region;
    private readonly EntityUid? _doll;
    private readonly Dictionary<int, List<MarkingTileLayer>> _inserts = new();
    private readonly List<MarkingTileLayer> _trailing = new();
    private readonly HashSet<int> _hidden = new();
    private readonly MarkingCategories? _hiddenCategory;

    public LayeredTextureControl(MarkingTileColors colors, List<MarkingTileLayer> layers, UIBox2 region, EntityUid? doll = null, MarkingCategories? hiddenCategory = null)
    {
        _hiddenCategory = hiddenCategory;
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
                DrawTexture(handle, LayerTexture(layer), _colors.Get(layer.Tint, layer.ColorLayer), scale);
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

        CollectHiddenLayers(spriteSystem, (doll, sprite));

        var i = 0;
        foreach (var spriteLayer in sprite.AllLayers)
        {
            if (spriteLayer.Visible && !_hidden.Contains(i) && !IsStencilOnly(spriteLayer) && GetTexture(spriteLayer) is { } texture)
                DrawTexture(handle, texture, spriteLayer.Color * sprite.Color, scale);

            if (_inserts.TryGetValue(i, out var inserted))
            {
                foreach (var layer in inserted)
                {
                    DrawTexture(handle, LayerTexture(layer), _colors.Get(layer.Tint, layer.ColorLayer), scale);
                }
            }

            i++;
        }

        foreach (var layer in _trailing)
        {
            DrawTexture(handle, LayerTexture(layer), _colors.Get(layer.Tint, layer.ColorLayer), scale);
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

    private void CollectHiddenLayers(SpriteSystem spriteSystem, Entity<SpriteComponent> doll)
    {
        _hidden.Clear();

        if (_hiddenCategory is not { } category
            || !_entMan.TryGetComponent<HumanoidAppearanceComponent>(doll, out var humanoid)
            || !humanoid.MarkingSet.Markings.TryGetValue(category, out var markings))
            return;

        foreach (var marking in markings)
        {
            if (!_markingManager.Markings.TryGetValue(marking.MarkingId, out var proto))
                continue;

            foreach (var specifier in proto.Sprites)
            {
                if (specifier is SpriteSpecifier.Rsi rsi
                    && spriteSystem.LayerMapTryGet(doll.AsNullable(), $"{proto.ID}-{rsi.RsiState}", out var index, false))
                    _hidden.Add(index);
            }
        }
    }

    private Texture LayerTexture(MarkingTileLayer layer)
    {
        if (layer.Animation is not { IsAnimated: true } animation)
            return layer.Texture;

        var total = 0f;
        for (var i = 0; i < animation.AnimationFrameCount; i++)
        {
            total += animation.GetDelay(i);
        }

        if (total <= 0)
            return layer.Texture;

        var time = (float) (_timing.RealTime.TotalSeconds % total);
        for (var i = 0; i < animation.AnimationFrameCount; i++)
        {
            time -= animation.GetDelay(i);
            if (time < 0)
                return animation.GetFrame(RsiDirection.South, i);
        }

        return layer.Texture;
    }

    private static bool IsStencilOnly(ISpriteLayer layer)
    {
        return layer is SpriteComponent.Layer { ShaderPrototype: { } shader } && shader == StencilClear;
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
