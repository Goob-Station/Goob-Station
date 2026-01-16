using Content.Goobstation.Client.Heretic.UI;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Heretic;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Heretic;

public sealed partial class HereticOverlaySystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentStartup>(OnEntropicPlumeAffectedStartup);
        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentShutdown>(OnEntropicPlumeAffectedShutdown);

        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentStartup>(OnCombatMarkStartup);
        SubscribeLocalEvent<HereticCombatMarkComponent, AfterAutoHandleStateEvent>(OnCombatMarkAfterAutoHandleState);
        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentShutdown>(OnCombatMarkShutdown);

        SubscribeLocalEvent<FireBlastedComponent, ComponentStartup>(OnFireBlastedStartup);
        SubscribeLocalEvent<FireBlastedComponent, ComponentShutdown>(OnFireBlastedShutdown);

        SubscribeLocalEvent<ShadowCloakedComponent, ComponentStartup>(OnShadowCloakedStartup);
        SubscribeLocalEvent<ShadowCloakedComponent, ComponentShutdown>(OnShadowCloakedShutdown);

        SubscribeLocalEvent<StarMarkComponent, ComponentStartup>(OnStarMarkStartup);
        SubscribeLocalEvent<StarMarkComponent, ComponentShutdown>(OnStarMarkShutdown);

        _overlay.AddOverlay(new VoidConduitOverlay());
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<VoidConduitOverlay>();
    }

    private void OnEntropicPlumeAffectedShutdown(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite)
        || !_sprite.LayerMapTryGet((ent, sprite), EntropicPlumeKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer((ent, sprite), layer);
    }

    private void OnEntropicPlumeAffectedStartup(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite)
        || _sprite.LayerMapTryGet((ent, sprite), EntropicPlumeKey.Key, out _, false))
            return;

        var layer = _sprite.AddLayer((ent, sprite), ent.Comp.Sprite);
        _sprite.LayerMapSet((ent, sprite), EntropicPlumeKey.Key, layer);
    }

    private void CombatMarkAddLayer(Entity<HereticCombatMarkComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var state = ent.Comp.Path.ToLower();
        int? index = state == "cosmos" ? 0 : null; // Cosmos mark should be behind the sprite

        if (_sprite.LayerMapTryGet((ent, sprite), HereticCombatMarkKey.Key, out var layer, false))
        {
            if (index == 0)
                _sprite.RemoveLayer((ent, sprite), layer);
            else
            {
                _sprite.LayerSetRsiState((ent, sprite), layer, state);
                return;
            }
        }

        var rsi = new SpriteSpecifier.Rsi(ent.Comp.ResPath, state);

        layer = _sprite.AddLayer((ent, sprite), rsi, index);
        _sprite.LayerMapSet((ent, sprite), HereticCombatMarkKey.Key, layer);
        sprite.LayerSetShader(layer, "unshaded");
    }

    private void OnCombatMarkStartup(Entity<HereticCombatMarkComponent> ent, ref ComponentStartup args)
    {
        CombatMarkAddLayer(ent);
    }

    private void OnCombatMarkAfterAutoHandleState(Entity<HereticCombatMarkComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        CombatMarkAddLayer(ent);
    }

    private void OnCombatMarkShutdown(Entity<HereticCombatMarkComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite)
        || !_sprite.LayerMapTryGet((ent, sprite), HereticCombatMarkKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer((ent, sprite), layer);
    }

    private void OnFireBlastedStartup(Entity<FireBlastedComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite)
        || _sprite.LayerMapTryGet((uid, sprite), FireBlastedKey.Key, out _, false))
            return;

        var layer = _sprite.AddLayer((uid, sprite), comp.Sprite);
        _sprite.LayerMapSet((uid, sprite), FireBlastedKey.Key, layer);
    }

    private void OnFireBlastedShutdown(Entity<FireBlastedComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite)
        || !_sprite.LayerMapTryGet((uid, sprite), FireBlastedKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer((uid, sprite), layer);
    }

    private void OnShadowCloakedStartup(Entity<ShadowCloakedComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        ent.Comp.WasVisible = sprite.Visible;
        _sprite.SetVisible((ent, sprite), false);
    }

    private void OnShadowCloakedShutdown(Entity<ShadowCloakedComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        _sprite.SetVisible((ent, sprite), ent.Comp.WasVisible);
    }

    private void OnStarMarkStartup(Entity<StarMarkComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (_sprite.LayerMapTryGet((uid, sprite), StarMarkKey.Key, out _, false))
            return;

        var layer = _sprite.AddLayer((uid, sprite), comp.Sprite);
        sprite.LayerSetShader(layer, "unshaded");
        _sprite.LayerMapSet((uid, sprite), StarMarkKey.Key, layer);
    }

    private void OnStarMarkShutdown(Entity<StarMarkComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((uid, sprite), StarMarkKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer((uid, sprite), layer);
    }


}
