using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._White.Xenomorphs.Screech;

public sealed class SharedXenomorphScreechSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenomorphScreechComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<XenomorphScreechComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphScreechComponent, XenomorphScreechActionEvent>(OnScreech);
    }

    private void OnMapInit(EntityUid uid, XenomorphScreechComponent component, MapInitEvent args) =>
        _actions.AddAction(uid, ref component.ActionEntity, component.ActionId);

    private void OnShutdown(EntityUid uid, XenomorphScreechComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.ActionEntity);

    private void OnScreech(EntityUid uid, XenomorphScreechComponent component, XenomorphScreechActionEvent args)
    {
        // CanAttack fails when rooted/static (ovipositor attached); consciousness is enough.
        if (args.Handled || !_blocker.CanConsciouslyPerformAction(uid))
            return;

        args.Handled = true;

        // Server-only: shared InstantAction would otherwise PlayPvs twice (client+server) and garble the sample.
        if (_net.IsServer)
        {
            _audio.PlayPvs(component.Sound, uid, AudioParams.Default.WithVolume(3f).WithMaxDistance(28f));
            _popup.PopupEntity(Loc.GetString("xenomorphs-screech"), uid, uid);

            if (component.EffectPrototype is { } effectProto)
            {
                var effect = Spawn(effectProto, Transform(uid).Coordinates);
                _transform.SetParent(effect, uid);
            }
        }

        if (!_net.IsServer)
            return;

        var mapCoords = _transform.GetMapCoordinates(uid);
        foreach (var found in _lookup.GetEntitiesInRange<MobStateComponent>(mapCoords, component.Range))
        {
            var target = found.Owner;
            if (target == uid)
                continue;

            if (HasComp<XenomorphComponent>(target))
                continue;

            _stun.TryKnockdown(target, component.StunTime, refresh: true, autoStand: true, drop: true, force: true);
            _stun.TryUpdateParalyzeDuration(target, component.StunTime);
        }
    }
}
