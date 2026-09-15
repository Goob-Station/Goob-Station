using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Shared.Voidwalker.Abilities.Unsettle;
using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Goobstation.Shared.Voidwalker.GlassPasser;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
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

    public override void Initialize()
    {
        SubscribeLocalEvent<VoidwalkerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VoidwalkerComponent, GridUidChangedEvent>(OnGridUidChanged);

        SubscribeLocalEvent<VoidwalkerComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerSpacedStatusChangedEvent>(OnSpacedStatusChanged);

        SubscribeLocalEvent<VoidwalkerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<VoidwalkerComponent, PullStoppedMessage>(OnPullStopped);
    }

    private void OnStartup(Entity<VoidwalkerComponent> entity, ref ComponentStartup args)
    {
        UpdateSpacedStatus(entity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<VoidwalkerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime > comp.NextSpacedCheck)
            {
                UpdateSpacedStatus((uid, comp));
                comp.NextSpacedCheck = curTime + comp.SpacedCheckInterval;
            }

            if (curTime >= comp.NextHealingTick && comp is { IsInSpace: true, HealingWhenSpaced: { } healing })
            {
                _damageable.TryChangeDamage(uid, healing);
                comp.NextHealingTick = curTime + comp.HealingTickInterval;
            }

        }
    }

    private void OnGridUidChanged(Entity<VoidwalkerComponent> entity, ref GridUidChangedEvent args) =>
        UpdateSpacedStatus(entity);

    private void OnSpacedStatusChanged(Entity<VoidwalkerComponent> entity, ref VoidwalkerSpacedStatusChangedEvent args)
    {
        if (TerminatingOrDeleted(entity))
            return;

        if (args.Spaced
            && TryComp<VoidwalkerUnsettleComponent>(entity, out var unsettle)
            && unsettle.UnsettleDoAfterId == null) // if spaced and not currently performing unsettle, go invis
        {
            EnsureComp<StealthComponent>(entity); // Okay, this is a weird way to do this, but stealth literally doesn't work if you enable/disable it so IDK :shrug:
           _stealth.SetThermalsImmune(entity, args.Spaced); // tell me if you find a better way to fix this - delph
        }
        else
            RemComp<StealthComponent>(entity);

        _movement.RefreshMovementSpeedModifiers(entity);
        Dirty(entity);
    }

    private void OnRefreshMoveSpeed(Entity<VoidwalkerComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var modifier = ent.Comp.IsInSpace ? 1f : ent.Comp.NonSpacedSpeedModifier;
        args.ModifySpeed(modifier, modifier);
        Dirty(ent);
    }

    #region Helpers

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
            && !voidwalker.IsInSpace)
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

    #endregion

    #region Updating Spaced Status
    public void UpdateSpacedStatus(Entity<VoidwalkerComponent> entity)
    {
        var spaced = CheckIfSpaced(entity);
        entity.Comp.IsInSpace = spaced;

        var ev = new VoidwalkerSpacedStatusChangedEvent(spaced);
        RaiseLocalEvent(entity, ref ev);
    }

    public bool CheckIfSpaced(EntityUid entity)
    {
        var ev = new VoidwalkerCheckTileSpacedStatusEvent();
        RaiseLocalEvent(entity, ref ev);
        return ev.Spaced;
    }


    #endregion

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
