using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Goobstation.Shared.GrabIntent;
using Content.Goobstation.Shared.Voidwalker.Abilities.Unsettle;
using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Goobstation.Shared.Voidwalker.GlassPasser;
using Content.Shared.Actions;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Shared.Traits.Assorted;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Voidwalker;

public partial class SharedVoidwalkerSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidwalkerComponent, GridUidChangedEvent>(OnGridUidChanged);

        SubscribeLocalEvent<VoidwalkerComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerSpacedStatusChangedEvent>(OnSpacedStatusChanged);

        SubscribeLocalEvent<VoidwalkerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<VoidwalkerComponent, PullStoppedMessage>(OnPullStopped);
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


    #endregion

    #region Updating Spaced Status
    public void UpdateSpacedStatus(Entity<VoidwalkerComponent> entity)
    {
        var isInSpace = CheckInSpace(entity.Owner, entity.Comp);
        entity.Comp.IsInSpace = isInSpace;

        var ev = new VoidwalkerSpacedStatusChangedEvent(isInSpace);
        RaiseLocalEvent(entity, ref ev);
    }
    public bool CheckInSpace(EntityUid uid, VoidwalkerComponent? voidwalker = null)
    {
        var entityXform = Transform(uid);

        // Check if the voidwalker is standing inside a passed object.
        // is this hacky? Yes. Very.
        if (voidwalker is not null
            && TryComp<GlassPasserComponent>(uid, out var glassPasser))
            foreach (var (entityPassed, _) in glassPasser.EntitiesPassed)
                if (_transform.InRange(uid, entityPassed, voidwalker.PassedObjectGraceRange))
                    return false;

        // If the voidwalker is not on a grid, it is in space.
        if (entityXform.GridUid is not { } gridUid)
            return true;

        /*
        // If the voidwalker *is* on a grid, but the grid has no atmosphere; it is in space.
        var position = _transform.GetGridOrMapTilePosition(uid);
        var tileMixture = _atmos.GetTileMixture(gridUid, entityXform.MapUid, position);

        return tileMixture is null || tileMixture.Pressure <= 0;
        */ // idk how to get this working on shared so hmu if u know

        return false;
    }

    #endregion

    #region Dragging

    /// <summary>
    /// We apply pressure immunity to a target being dragged by a voidwalker so they have time to kidnap them
    /// without them dying to pressure.
    /// </summary>
    private void OnPullStarted(Entity<VoidwalkerComponent> entity, ref PullStartedMessage args)
    {
        entity.Comp.EntityPulledWasSpaceImmune = HasComp<SpecialPressureImmunityComponent>(args.PulledUid);
        EnsureComp<SpecialPressureImmunityComponent>(args.PulledUid);
    }

    private void OnPullStopped(Entity<VoidwalkerComponent> entity, ref PullStoppedMessage args)
    {
        if (!entity.Comp.EntityPulledWasSpaceImmune)
            RemComp<SpecialPressureImmunityComponent>(args.PulledUid);
    }

    #endregion

}
