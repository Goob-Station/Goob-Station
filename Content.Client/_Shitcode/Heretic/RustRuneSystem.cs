using System.Numerics;
using Content.Client.IconSmoothing;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Tag;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class RustRuneSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RustRuneComponent, ComponentStartup>(OnStartup, after: new[] { typeof(IconSmoothSystem) });
        SubscribeLocalEvent<RustRuneComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<RustRuneComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);

        SubscribeLocalEvent<SpriteRandomOffsetComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<SpriteRandomOffsetComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !args.AppearanceData.TryGetValue(OffsetVisuals.Offset, out var offset))
            return;

        args.Sprite.Offset = (Vector2) offset;
    }

    private void OnAfterAutoHandleState(Entity<RustRuneComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var diagonal = _tag.HasTag(uid, comp.DiagonalTag);

        if (comp.RustOverlay && !sprite.LayerMapTryGet(RustRuneKey.Overlay, out _))
        {
            var layer = sprite.AddLayer(diagonal ? comp.DiagonalSprite : comp.OverlaySprite);
            sprite.LayerMapSet(RustRuneKey.Overlay, layer);
        }

        if (comp.RuneIndex >= 0 && comp.RuneIndex < comp.RuneSprites.Count)
        {
            if (!sprite.LayerMapTryGet(RustRuneKey.Rune, out var layer))
            {
                layer = sprite.AddLayer(comp.RuneSprites[comp.RuneIndex]);
                sprite.LayerMapSet(RustRuneKey.Rune, layer);
                sprite.LayerSetShader(RustRuneKey.Rune, "unshaded");
            }
            else
                sprite.LayerSetSprite(layer, comp.RuneSprites[comp.RuneIndex]);

            var offset = diagonal ? comp.DiagonalOffset : comp.RuneOffset;
            sprite.LayerSetOffset(layer, offset);
        }
    }

    private void OnShutdown(Entity<RustRuneComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (sprite.LayerMapTryGet(RustRuneKey.Rune, out var rune))
            sprite.RemoveLayer(rune);

        if (sprite.LayerMapTryGet(RustRuneKey.Overlay, out var overlay))
            sprite.RemoveLayer(overlay);
    }

    private void OnStartup(Entity<RustRuneComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        var diagonal = _tag.HasTag(uid, comp.DiagonalTag);

        if (comp.RustOverlay && !sprite.LayerMapTryGet(RustRuneKey.Overlay, out _))
        {
            var layer = sprite.AddLayer(diagonal ? comp.DiagonalSprite : comp.OverlaySprite);
            sprite.LayerMapSet(RustRuneKey.Overlay, layer);
        }

        if (comp.RuneIndex >= 0 && comp.RuneIndex < comp.RuneSprites.Count &&
            !sprite.LayerMapTryGet(RustRuneKey.Rune, out _))
        {
            var layer = sprite.AddLayer(comp.RuneSprites[comp.RuneIndex]);
            sprite.LayerMapSet(RustRuneKey.Rune, layer);
            sprite.LayerSetShader(RustRuneKey.Rune, "unshaded");
            var offset = diagonal ? comp.DiagonalOffset : comp.RuneOffset;
            sprite.LayerSetOffset(layer, offset);
        }
    }
}
