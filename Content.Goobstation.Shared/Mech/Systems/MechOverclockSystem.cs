using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Mech.Systems;

/// <summary>
/// Allows the mech to move faster while it's overclocked.
/// While overclocked it gets increased power drain and loses integrity.
/// </summary>
public sealed class MechOverclockSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMechSystem _mech = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechComponent, MechOverclockActionEvent>(OnToggle);
        SubscribeLocalEvent<MechOverclockComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<MechOverclockComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MechOverclockComponent, EntInsertedIntoContainerMessage>(OnPilotInserted);
    }

    private void OnPilotInserted(Entity<MechOverclockComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_net.IsClient || !TryComp<MechComponent>(ent, out var mech) || args.Container.ID != mech.PilotSlotId)
            return;

        _actions.AddAction(args.Entity, ref ent.Comp.ActionEntity, ent.Comp.ActionProto, ent);
        _actions.SetToggled(ent.Comp.ActionEntity, ent.Comp.Active);
        Dirty(ent);
    }

    private void OnShutdown(Entity<MechOverclockComponent> ent, ref ComponentShutdown args)
    {
        ClearGlow(ent);
        _actions.RemoveAction(ent.Comp.ActionEntity);
        PredictedQueueDel(ent.Comp.ActionEntity);
    }

    private void OnToggle(Entity<MechComponent> ent, ref MechOverclockActionEvent args)
    {
        if (args.Handled || !TryComp<MechOverclockComponent>(ent, out var overclock))
            return;

        args.Handled = true;

        if (!overclock.Active && ent.Comp.Energy <= 0)
        {
            _popup.PopupClient(Loc.GetString("mech-ability-no-power"), ent, args.Performer);
            return;
        }

        SetActive((ent, overclock), !overclock.Active);
        _popup.PopupClient(Loc.GetString(overclock.Active ? "mech-overclock-on" : "mech-overclock-off", ("mech", ent.Owner)), ent, args.Performer);
    }

    private void OnRefreshSpeed(Entity<MechOverclockComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Active)
            args.ModifySpeed(ent.Comp.ActiveSpeedMultiplier, ent.Comp.ActiveSpeedMultiplier);
    }

    public void SetActive(Entity<MechOverclockComponent> ent, bool active)
    {
        if (ent.Comp.Active == active)
            return;

        ent.Comp.Active = active;
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateInterval;

        ClearGlow(ent);
        if (active)
            ent.Comp.GlowEntity = PredictedSpawnAttachedTo(ent.Comp.GlowProto, new EntityCoordinates(ent, default));

        Dirty(ent);

        _actions.SetToggled(ent.Comp.ActionEntity, active);
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    private void ClearGlow(Entity<MechOverclockComponent> ent)
    {
        if (ent.Comp.GlowEntity is { } glow)
            PredictedQueueDel(glow);

        ent.Comp.GlowEntity = null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<MechOverclockComponent, MechComponent>();
        while (query.MoveNext(out var uid, out var overclock, out var mech))
        {
            if (!overclock.Active || curTime < overclock.NextUpdate)
                continue;

            overclock.NextUpdate = curTime + overclock.UpdateInterval;
            Dirty(uid, overclock);

            if (mech.Broken || mech.PilotSlot.ContainedEntity == null || mech.Energy <= 0)
            {
                SetActive((uid, overclock), false);
                continue;
            }

            var seconds = (float) overclock.UpdateInterval.TotalSeconds;
            var drain = FixedPoint2.Min(overclock.OverclockedPowerDrainPerSecond * seconds, mech.Energy);
            _mech.TryChangeEnergy(uid, -drain, mech);

            _damageable.TryChangeDamage(uid, overclock.DamagePerSecond * seconds, true, false);
        }
    }
}
