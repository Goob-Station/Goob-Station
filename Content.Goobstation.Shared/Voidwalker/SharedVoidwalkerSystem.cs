using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.Voidwalker.Abilities.Unsettle;
using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Goobstation.Shared.Voidwalker.GlassPasser;
using Content.Goobstation.Shared.Voidwalker.Spaced;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Throwing;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Voidwalker;

/// <summary>
/// Handles like... everything else about Voidwalkers.
/// </summary>
public sealed partial class SharedVoidwalkerSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VoidwalkerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VoidwalkerComponent, GridUidChangedEvent>(OnGridUidChanged);

        SubscribeLocalEvent<VoidwalkerComponent, ThrowEvent>(OnThrow);
        SubscribeLocalEvent<VoidwalkerComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);

        SubscribeLocalEvent<VoidwalkerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<VoidwalkerComponent, PullStoppedMessage>(OnPullStopped);
    }

    private void OnStartup(Entity<VoidwalkerComponent> entity, ref ComponentStartup args)
    {
        var spaced = EnsureComp<SpacedStatusComponent>(entity);
        spaced.IsInSpace = false;
        HandleSpaceStatus(entity, false);

        UpdateSpacedStatus(entity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<VoidwalkerComponent, SpacedStatusComponent>();
        while (query.MoveNext(out var uid, out var voidwalker, out var spaced))
        {
            if (spaced.Changed)
            {
                HandleSpaceStatus((uid, voidwalker), spaced.IsInSpace);
                spaced.Changed = false;
            }

            if (curTime < voidwalker.NextHealingTick
                || !spaced.IsInSpace
                || voidwalker.HealingWhenSpaced is not { } healing)
                continue;

            _damageable.TryChangeDamage(uid, healing);
            voidwalker.NextHealingTick = curTime + voidwalker.HealingTickInterval;
        }
    }

    private void OnGridUidChanged(Entity<VoidwalkerComponent> entity, ref GridUidChangedEvent args) =>
        UpdateSpacedStatus(entity);

    // we update the speed modifiers on throw otherwise it fucks our modifier for some reason
    private void OnThrow(Entity<VoidwalkerComponent> entity, ref ThrowEvent args) =>
        _movement.RefreshMovementSpeedModifiers(entity);

    public void UpdateSpacedStatus(Entity<VoidwalkerComponent> entity)
    {
        var ev = new CheckSpacedStatusEvent();
        RaiseLocalEvent(entity, ref ev);

        // Check if the voidwalker is standing inside a passed object.
        if (TryComp<GlassPasserComponent>(entity, out var glassPasser))
        {
            foreach (var (entityPassed, _) in glassPasser.EntitiesPassed)
            {
                if (_transform.InRange(entity.Owner, entityPassed, entity.Comp.PassedObjectGraceRange))
                {
                    ev.Spaced = true;
                    ev.Changed = true;
                }
            }
        }

        if (ev.Changed)
            HandleSpaceStatus(entity, ev.Spaced);
    }
    private void HandleSpaceStatus(Entity<VoidwalkerComponent> entity, bool spaced)
    {
        if (TerminatingOrDeleted(entity))
            return;

        _stealth.SetEnabled(entity, spaced);
        _stealth.SetThermalsImmune(entity, spaced);

        _movement.RefreshMovementSpeedModifiers(entity);
        Dirty(entity);
    }

    private void OnRefreshMoveSpeed(Entity<VoidwalkerComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<SpacedStatusComponent>(ent, out var spaced))
            return;

        var modifier = spaced.IsInSpace ? 1f : ent.Comp.NonSpacedSpeedModifier;
        args.ModifySpeed(modifier, modifier);
    }
    public bool TryUseAbility(EntityUid entity, BaseActionEvent action, VoidwalkerComponent? voidwalker = null)
    {
        if (action.Handled)
            return false;

        if (!Resolve(entity, ref voidwalker))
            return true;

        UpdateSpacedStatus((entity, voidwalker));

        if (!TryComp<VoidwalkerActionComponent>(action.Action, out var voidwalkerAction))
            return false;

        if (voidwalkerAction.RequireInSpace
            && TryComp<SpacedStatusComponent>(entity, out var spaced)
            && !spaced.IsInSpace)
        {
            var popup = Loc.GetString("voidwalker-action-fail-require-in-space");
            _popup.PopupClient(popup, entity, entity);

            return false;
        }

        action.Handled = true;

        return true;
    }

    public bool CanSeeVoidwalker(EntityUid target)
    {
        if (HasComp<PermanentBlindnessComponent>(target)
            || HasComp<TemporaryBlindnessComponent>(target))
            return false;

        return !TryComp<BlindableComponent>(target, out var blindable)
               || blindable.EyeDamage < blindable.MaxDamage;
    }


    private T EnsureTrackedComp<T>(EntityUid uid, Entity<VoidwalkerComponent> ent, T? preconfigured = null) where T : Component, new()
    {
        if (TryComp<T>(uid, out var existing))
            return existing;

        var componentName = typeof(T).FullName;
        if (componentName != null)
            ent.Comp.AddedVoidwalkerComponents.Add(componentName);

        if (preconfigured != null)
        {
            AddComp(uid, preconfigured);
            return preconfigured;
        }

        return EnsureComp<T>(uid);
    }

    /// <summary>
    /// Removes tracked components.
    /// </summary>
    private void RemoveTrackedComps(EntityUid uid, Entity<VoidwalkerComponent> ent)
    {
        if (ent.Comp.AddedVoidwalkerComponents.Count == 0)
            return;

        foreach (var component in EntityManager.GetComponents(uid))
        {
            var componentName = component.GetType().FullName;
            if (componentName != null && ent.Comp.AddedVoidwalkerComponents.Contains(componentName))
                RemCompDeferred(uid, component.GetType());
        }

        ent.Comp.AddedVoidwalkerComponents.Clear();
    }

    #region Dragging

    /// <summary>
    /// We apply pressure immunity to a target being dragged by a voidwalker so they have time to kidnap them
    /// without them dying to pressure.
    /// </summary>
    private void OnPullStarted(Entity<VoidwalkerComponent> entity, ref PullStartedMessage args)
    {
        EnsureTrackedComp<SpecialPressureImmunityComponent>(args.PulledUid, entity);
    }

    private void OnPullStopped(Entity<VoidwalkerComponent> entity, ref PullStoppedMessage args)
    {
        RemoveTrackedComps(args.PulledUid, entity);
    }

    #endregion

}
