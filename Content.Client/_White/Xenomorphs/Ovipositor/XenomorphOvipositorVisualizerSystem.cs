using Content.Client.DamageState;
using Content.Shared._White.Xenomorphs.Ovipositor;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Client.GameObjects;

namespace Content.Client._White.Xenomorphs.Ovipositor;

public sealed class XenomorphOvipositorVisualizerSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenomorphOvipositorComponent, AfterAutoHandleStateEvent>(OnAfterState);
        SubscribeLocalEvent<XenomorphOvipositorComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<XenomorphOvipositorComponent, AppearanceChangeEvent>(OnAppearance,
            after: [typeof(DamageStateVisualizerSystem)]);
    }

    private void OnAfterState(EntityUid uid, XenomorphOvipositorComponent component, ref AfterAutoHandleStateEvent args) =>
        UpdateSprite(uid, component);

    private void OnMobStateChanged(EntityUid uid, XenomorphOvipositorComponent component, MobStateChangedEvent args) =>
        UpdateSprite(uid, component);

    private void OnAppearance(EntityUid uid, XenomorphOvipositorComponent component, ref AppearanceChangeEvent args) =>
        UpdateSprite(uid, component, args.Component, args.Sprite);

    private void UpdateSprite(
        EntityUid uid,
        XenomorphOvipositorComponent component,
        AppearanceComponent? appearance = null,
        SpriteComponent? sprite = null)
    {
        if (!_sprite.LayerMapTryGet((uid, sprite), DamageStateVisualLayers.Base, out _, false))
            return;

        if (TryComp<MobStateComponent>(uid, out var mob) && mob.CurrentState != MobState.Alive)
            return;

        var attached = component.Attached;
        if (_appearance.TryGetData(uid, XenomorphOvipositorVisuals.Attached, out bool appearanceAttached, appearance))
            attached = appearanceAttached;

        if (attached)
        {
            _sprite.LayerSetRsi((uid, sprite), DamageStateVisualLayers.Base,
                component.AttachedSpriteRsi, component.AttachedSpriteState);
        }
        else
        {
            _sprite.LayerSetRsi((uid, sprite), DamageStateVisualLayers.Base,
                component.DetachedSpriteRsi, component.DetachedSpriteState);
        }
    }
}
