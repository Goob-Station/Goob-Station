using System.Numerics;
using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Actions;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Mech.Systems;

public sealed partial class MechStompSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private SharedMechSystem _mech = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedCameraRecoilSystem _recoil = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechComponent, MechStompActionEvent>(OnStomp);
        SubscribeLocalEvent<MechStompComponent, MechStompDoAfterEvent>(OnStompFinished);
        SubscribeLocalEvent<MechStompComponent, EntInsertedIntoContainerMessage>(OnPilotInserted);
        SubscribeLocalEvent<MechStompComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnPilotInserted(Entity<MechStompComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_net.IsClient || !TryComp<MechComponent>(ent, out var mech) || args.Container.ID != mech.PilotSlotId)
            return;

        _actions.AddAction(args.Entity, ref ent.Comp.ActionEntity, ent.Comp.ActionProto, ent);
        Dirty(ent);
    }

    private void OnShutdown(Entity<MechStompComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEntity);
        QueueDel(ent.Comp.ActionEntity);
    }

    private void OnStomp(Entity<MechComponent> ent, ref MechStompActionEvent args)
    {
        if (args.Handled || ent.Comp.Broken || !TryComp<MechStompComponent>(ent, out var stomp) || stomp.ImpactTime != null)
            return;

        var pilot = args.Performer;
        if (ent.Comp.Energy < stomp.PowerCageEnergyCost)
        {
            _popup.PopupClient(Loc.GetString("mech-ability-no-power"), ent, pilot);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, pilot, stomp.StompWindUpDoAfter, new MechStompDoAfterEvent(), ent, target: ent)
        {
            BreakOnMove = true,
            BreakOnDamage = false,
            BreakOnHandChange = false,
            NeedHand = false,
            RequireCanInteract = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        args.Handled = true;
        _audio.PlayPredicted(stomp.WindUpSound, ent, pilot);
        _popup.PopupPredicted(Loc.GetString("mech-stomp-windup", ("mech", ent.Owner)), ent, pilot, PopupType.Medium);
    }

    private void OnStompFinished(Entity<MechStompComponent> ent, ref MechStompDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var mech = ent.Owner;
        if (!TryComp<MechComponent>(mech, out var mechComp)
            || mechComp.Broken
            || mechComp.Energy < ent.Comp.PowerCageEnergyCost)
            return;

        args.Handled = true;
        _mech.TryChangeEnergy(mech, -ent.Comp.PowerCageEnergyCost, mechComp);

        if (_net.IsClient)
            return;

        var xform = Transform(mech);
        var overlay = EnsureComp<MechStompOverlayComponent>(mech);
        overlay.DustRange = ent.Comp.StompRadius;
        overlay.Origin = _transform.GetMoverCoordinates(mech, xform);
        Dirty(mech, overlay);

        ent.Comp.ImpactTime = _timing.CurTime + overlay.SpriteLiftTiming;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<MechStompComponent, MechComponent>();
        while (query.MoveNext(out var uid, out var stomp, out var mech))
        {
            if (stomp.ImpactTime is not { } impact || curTime < impact)
                continue;

            stomp.ImpactTime = null;
            Dirty(uid, stomp);

            if (!mech.Broken)
                Impact((uid, stomp), mech);
        }
    }

    private void Impact(Entity<MechStompComponent> ent, MechComponent mechComp)
    {
        var mech = ent.Owner;
        var xform = Transform(mech);
        var (origin, facing) = _transform.GetWorldPositionRotation(xform);

        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(xform.Coordinates, ent.Comp.StompRadius))
        {
            if (target.Owner == mech || target.Owner == mechComp.PilotSlot.ContainedEntity)
                continue;

            _stun.TryKnockdown(target.Owner, ent.Comp.KnockdownTime);
            _damageable.TryChangeDamage(target.Owner, ent.Comp.Damage, origin: mech);

            var away = _transform.GetWorldPosition(target) - origin;
            var dir = away.LengthSquared() > 0.01f ? Vector2.Normalize(away) : facing.ToWorldVec();
            _throwing.TryThrow(target.Owner, dir, ent.Comp.KnockbackSpeed, mech, playSound: false);
        }

        _popup.PopupPredicted(Loc.GetString("mech-stomp-popup", ("mech", mech)), mech, mechComp.PilotSlot.ContainedEntity, PopupType.MediumCaution);

        foreach (var camera in _lookup.GetEntitiesInRange<CameraRecoilComponent>(xform.Coordinates, ent.Comp.StompRadius * 4f))
        {
            var delta = _transform.GetWorldPosition(camera) - origin;
            var dir = delta.LengthSquared() > 0.01f ? Vector2.Normalize(delta) : facing.ToWorldVec();
            var falloff = 1f - Math.Clamp(delta.Length() / (ent.Comp.StompRadius * 4f), 0f, 0.8f);
            _recoil.KickCamera(camera, dir * ent.Comp.CameraKick * falloff);
        }
    }
}
