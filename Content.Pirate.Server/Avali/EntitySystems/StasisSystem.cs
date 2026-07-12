using Content.Pirate.Shared.Avali.Components;
using Content.Pirate.Shared.Avali.EntitySystems;
using Content.Pirate.Shared.Avali.Events;
using Content.Server.Body.Systems;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Pirate.Server.Avali.EntitySystems;

/// <summary>
/// Server-side nanite stasis healing, damage reduction, and action state.
/// </summary>
public sealed class StasisSystem : SharedStasisSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StasisComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StasisComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<StasisComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<StasisComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<StasisComponent, PrepareStasisActionEvent>(OnPrepareStasis);
        SubscribeLocalEvent<StasisComponent, EnterStasisActionEvent>(OnEnterStasis);
        SubscribeLocalEvent<StasisComponent, ExitStasisActionEvent>(OnExitStasis);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;
        var query = EntityQueryEnumerator<StasisComponent>();
        while (query.MoveNext(out var uid, out var stasis))
        {
            if (!stasis.IsInStasis || stasis.NextHeal > currentTime)
                continue;

            var modifier = _mobState.IsCritical(uid) ? -stasis.CritHealingModifier : -1.0f;
            _damageable.TryChangeDamage(uid, modifier * stasis.HealingPerUpdate, true, origin: uid);
            _bloodstream.TryModifyBleedAmount(uid, modifier * stasis.BleedHealPerUpdate);

            stasis.NextHeal += stasis.UpdateInterval;
        }
    }

    private void OnMapInit(EntityUid uid, StasisComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.EnterStasisActionEntity, component.EnterStasisAction);
    }

    private void OnComponentShutdown(EntityUid uid, StasisComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.EnterStasisActionEntity);
        _actions.RemoveAction(uid, component.ExitStasisActionEntity);
        RemComp<StasisFrozenComponent>(uid);
    }

    private void OnMobStateChanged(Entity<StasisComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead &&
            (ent.Comp.IsInStasis || HasComp<StasisFrozenComponent>(ent.Owner)))
            RaiseLocalEvent(ent.Owner, new ExitStasisActionEvent());
    }

    private static void OnDamageModify(Entity<StasisComponent> ent, ref DamageModifyEvent args)
    {
        if (!ent.Comp.IsInStasis || args.Origin == ent.Owner)
            return;

        var updatedDamage = new DamageSpecifier();
        foreach (var damage in args.Damage.DamageDict)
        {
            updatedDamage.DamageDict[damage.Key] = damage.Value > 0
                ? ent.Comp.StasisDamageReduction * damage.Value
                : damage.Value;
        }

        args.Damage = updatedDamage;
    }

    private void OnPrepareStasis(EntityUid uid, StasisComponent component, PrepareStasisActionEvent args)
    {
        if (component.IsInStasis || HasComp<StasisFrozenComponent>(uid))
            return;

        EnsureComp<StasisFrozenComponent>(uid);

        _actions.RemoveAction(uid, component.EnterStasisActionEntity);
        _actions.AddAction(uid, ref component.ExitStasisActionEntity, component.ExitStasisAction);
        _actions.SetCooldown(component.ExitStasisActionEntity, component.StasisEnterEffectLifetime);

        RaiseAnimationEvent(uid, StasisAnimationType.Prepare);

        Timer.Spawn(component.StasisEnterEffectLifetime, () =>
        {
            if (!TryComp<StasisComponent>(uid, out var current) ||
                current.IsInStasis ||
                !HasComp<StasisFrozenComponent>(uid))
                return;

            RaiseLocalEvent(uid, new EnterStasisActionEvent());
        });
    }

    private void OnEnterStasis(EntityUid uid, StasisComponent component, EnterStasisActionEvent args)
    {
        if (component.IsInStasis || !HasComp<StasisFrozenComponent>(uid))
            return;

        component.IsInStasis = true;
        component.NextHeal = _timing.CurTime;
        component.IsVisible = false;
        Dirty(uid, component);

        RaiseAnimationEvent(uid, StasisAnimationType.Enter);
    }

    private void OnExitStasis(EntityUid uid, StasisComponent component, ExitStasisActionEvent args)
    {
        if (!component.IsInStasis && !HasComp<StasisFrozenComponent>(uid))
            return;

        component.IsInStasis = false;
        component.IsVisible = true;
        Dirty(uid, component);

        _actions.RemoveAction(uid, component.ExitStasisActionEntity);
        _actions.AddAction(uid, ref component.EnterStasisActionEntity, component.EnterStasisAction);
        _actions.SetCooldown(component.EnterStasisActionEntity, component.StasisCooldown);

        RemComp<StasisFrozenComponent>(uid);
        RaiseAnimationEvent(uid, StasisAnimationType.Exit);
    }

    private void RaiseAnimationEvent(EntityUid uid, StasisAnimationType type)
    {
        var coordinates = GetNetCoordinates(Transform(uid).Coordinates);
        var ev = new StasisAnimationEvent(GetNetEntity(uid), coordinates, type);
        RaiseNetworkEvent(ev, Filter.Pvs(uid, entityManager: EntityManager));
    }
}
