using Content.Client.Stealth;
using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Events;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Gangwars;

public sealed class ClientGangColorSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangColorComponent, AfterAutoHandleStateEvent>(OnColorStateHandled);
        SubscribeLocalEvent<GangColorComponent, GangColorAppliedEvent>(OnColorApplied);
        SubscribeLocalEvent<GangColorComponent, EquipmentVisualsUpdatedEvent>(OnEquipmentVisualsUpdated);
        SubscribeLocalEvent<GangColorComponent, HeldVisualsUpdatedEvent>(OnHeldVisualsUpdated);
        SubscribeLocalEvent<GangColorComponent, BeforePostShaderRenderEvent>(OnShaderRender,
            after: [typeof(StealthSystem)]);
    }

    private void OnColorStateHandled(Entity<GangColorComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        ApplyItemColor(ent, ent.Comp.GangColor);
    }

    /// <summary>
    /// Applies the tint to predicted entities the moment their color is assigned in code, so they don't render
    /// untinted for a tick while waiting on the networked state.
    /// </summary>
    private void OnColorApplied(Entity<GangColorComponent> ent, ref GangColorAppliedEvent args)
    {
        ApplyItemColor(ent, ent.Comp.GangColor);
    }

    private void OnShaderRender(Entity<GangColorComponent> ent, ref BeforePostShaderRenderEvent args)
    {
        var color = ent.Comp.GangColor;
        if (color == Color.Transparent)
            return;

        _sprite.SetColor((ent.Owner, args.Sprite), color);
    }

    /// <summary>
    /// Colors hand-held sprites.
    /// </summary>
    private void OnHeldVisualsUpdated(Entity<GangColorComponent> ent, ref HeldVisualsUpdatedEvent args)
    {
        var color = ent.Comp.GangColor;
        if (color == Color.Transparent)
            return;

        foreach (var key in args.RevealedLayers)
            if (_sprite.LayerExists(args.User, key))
                _sprite.LayerSetColor(args.User, _sprite.LayerMapGet(args.User, key), color);
    }

    /// <summary>
    /// Colors equipped sprites.
    /// </summary>
    private void OnEquipmentVisualsUpdated(Entity<GangColorComponent> ent, ref EquipmentVisualsUpdatedEvent args)
    {
        var color = ent.Comp.GangColor;
        if (color == Color.Transparent)
            return;

        foreach (var key in args.RevealedLayers)
            if (_sprite.LayerExists(args.Equipee, key))
                _sprite.LayerSetColor(args.Equipee, _sprite.LayerMapGet(args.Equipee, key), color);
    }

    /// <summary>
    /// Colors icon sprites.
    /// </summary>
    private void ApplyItemColor(EntityUid item, Color color)
    {
        if (color == Color.Transparent)
            return;

        _sprite.SetColor(item, color);
    }
}
