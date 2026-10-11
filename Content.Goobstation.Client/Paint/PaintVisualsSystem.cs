using System.Numerics;
using Content.Goobstation.Shared.Paint;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;

namespace Content.Goobstation.Client.Paint;

public sealed partial class PaintVisualsSystem : EntitySystem
{
    private const string BackingLayer = "backing";
    private const string PaintLayer = "paint";
    private const string FrameLayer = "frame";
    private const string EaselBackingLayer = "canvas_backing";
    private const string EaselPaintLayer = "canvas_paint";
    private const string EaselFrameLayer = "canvas_frame";

    [Dependency] private PaintEaselSystem _easel = default!;
    [Dependency] private PaintSurfaceSystem _paint = default!;
    [Dependency] private SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaintCanvasVisualsComponent, ComponentStartup>(OnCanvasStartup);
        SubscribeLocalEvent<PaintCanvasVisualsComponent, AfterAutoHandleStateEvent>(OnCanvasState);
        SubscribeLocalEvent<PaintCanvasVisualsComponent, PaintTextureChangedEvent>(OnTextureChanged);
        SubscribeLocalEvent<PaintEaselComponent, ComponentStartup>(OnEaselStartup);
        SubscribeLocalEvent<PaintEaselComponent, EntInsertedIntoContainerMessage>(OnEaselInserted);
        SubscribeLocalEvent<PaintEaselComponent, EntRemovedFromContainerMessage>(OnEaselRemoved);
    }

    private void OnCanvasStartup(Entity<PaintCanvasVisualsComponent> ent, ref ComponentStartup args)
    {
        UpdateCanvas(ent);
    }

    private void OnCanvasState(Entity<PaintCanvasVisualsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateCanvas(ent);
    }

    private void OnTextureChanged(Entity<PaintCanvasVisualsComponent> ent, ref PaintTextureChangedEvent args)
    {
        if (_easel.TryGetEasel(ent, out var easel))
            UpdateEasel(easel);
    }

    private void OnEaselStartup(Entity<PaintEaselComponent> ent, ref ComponentStartup args)
    {
        UpdateEasel(ent);
    }

    private void OnEaselInserted(Entity<PaintEaselComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID == ent.Comp.Slot)
            UpdateEasel(ent);
    }

    private void OnEaselRemoved(Entity<PaintEaselComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID == ent.Comp.Slot)
            UpdateEasel(ent);
    }

    private void UpdateCanvas(Entity<PaintCanvasVisualsComponent> ent)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
        {
            Entity<SpriteComponent?> spriteEnt = (ent.Owner, sprite);
            if (_sprite.LayerMapTryGet(spriteEnt, BackingLayer, out var backing, false))
                _sprite.LayerSetRsi(spriteEnt, backing, ent.Comp.BackingSprite, ent.Comp.BackingState);

            if (_sprite.LayerMapTryGet(spriteEnt, PaintLayer, out var paint, false))
            {
                _sprite.LayerSetScale(spriteEnt, paint, ent.Comp.PaintScale);
                _sprite.LayerSetOffset(spriteEnt, paint, ent.Comp.PaintOffset);
            }

            if (_sprite.LayerMapTryGet(spriteEnt, FrameLayer, out var frame, false))
                SetFrame(spriteEnt, frame, ent.Comp, Vector2.One, Vector2.Zero);
        }

        if (_easel.TryGetEasel(ent, out var easel))
            UpdateEasel(easel);
    }

    private void UpdateEasel(Entity<PaintEaselComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        Entity<SpriteComponent?> spriteEnt = (ent.Owner, sprite);
        if (!_sprite.LayerMapTryGet(spriteEnt, EaselBackingLayer, out var backing, false)
            || !_sprite.LayerMapTryGet(spriteEnt, EaselPaintLayer, out var paint, false)
            || !_sprite.LayerMapTryGet(spriteEnt, EaselFrameLayer, out var frame, false))
            return;

        if (_easel.GetCanvas(ent) is not { } canvas || !TryComp<PaintCanvasVisualsComponent>(canvas, out var visuals))
        {
            _sprite.LayerSetVisible(spriteEnt, backing, false);
            _sprite.LayerSetVisible(spriteEnt, paint, false);
            _sprite.LayerSetVisible(spriteEnt, frame, false);
            return;
        }

        var scale = new Vector2(ent.Comp.CanvasScale, ent.Comp.CanvasScale);
        var offset = visuals.EaselOffset;

        _sprite.LayerSetRsi(spriteEnt, backing, visuals.BackingSprite, visuals.BackingState);
        _sprite.LayerSetScale(spriteEnt, backing, scale);
        _sprite.LayerSetOffset(spriteEnt, backing, offset);
        _sprite.LayerSetVisible(spriteEnt, backing, true);

        var texture = _paint.GetTexture(canvas);
        _sprite.LayerSetTexture(spriteEnt, paint, texture);
        _sprite.LayerSetScale(spriteEnt, paint, visuals.PaintScale * ent.Comp.CanvasScale);
        _sprite.LayerSetOffset(spriteEnt, paint, offset + visuals.PaintOffset * ent.Comp.CanvasScale);
        _sprite.LayerSetVisible(spriteEnt, paint, texture != null);

        SetFrame(spriteEnt, frame, visuals, scale, offset);
    }

    private void SetFrame(Entity<SpriteComponent?> sprite, int layer, PaintCanvasVisualsComponent visuals, Vector2 scale, Vector2 offset)
    {
        if (visuals.Frame is not { } material)
        {
            _sprite.LayerSetVisible(sprite, layer, false);
            return;
        }

        _sprite.LayerSetRsi(sprite, layer, visuals.FrameSprite, $"{visuals.SizeKey}frame_{material}");
        _sprite.LayerSetScale(sprite, layer, scale);
        _sprite.LayerSetOffset(sprite, layer, offset);
        _sprite.LayerSetVisible(sprite, layer, true);
    }
}
