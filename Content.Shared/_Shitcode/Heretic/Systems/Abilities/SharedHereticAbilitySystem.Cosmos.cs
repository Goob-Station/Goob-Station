using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Heretic;
using Content.Shared.Projectiles;
using Content.Shared.StatusEffect;
using Robust.Shared.Map;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeCosmos()
    {
        SubscribeLocalEvent<EventHereticCosmicRune>(OnCosmicRune);
        SubscribeLocalEvent<EventHereticStarTouch>(OnStarTouch);
        SubscribeLocalEvent<EventHereticStarBlast>(OnStarBlast);
        SubscribeLocalEvent<EventHereticCosmicExpansion>(OnExpansion);
        SubscribeLocalEvent<HereticAscensionCosmosEvent>(OnAscension);

        SubscribeLocalEvent<StarBlastComponent, ProjectileHitEvent>(OnHit);
        SubscribeLocalEvent<StarBlastComponent, EntityTerminatingEvent>(OnEntityTerminating);
    }

    private void OnAscension(HereticAscensionCosmosEvent args)
    {
        _eye.SetDrawFov(args.Heretic, args.Negative);
    }

    private void OnExpansion(EventHereticCosmicExpansion args)
    {
        if (!TryUseAbility(args))
            return;

        var ent = args.Performer;

        var coords = Transform(ent).Coordinates;

        Heretic.TryGetHereticComponent(ent, out var heretic, out _);
        var strength = heretic is {CurrentPath: "Cosmos"} ? heretic.PathStage : 10;

        _starMark.ApplyStarMarkInRange(coords, ent, args.Range);
        _starMark.SpawnCosmicFields(coords, 2, strength);

        PredictedSpawnAtPosition(args.Effect, coords);

        if (heretic is null or {Ascended: true, CurrentPath: "Cosmos"})
        {
            _starMark.SpawnCosmicFieldLine(coords, DirectionFlag.North, -4, 4, 3, strength);
            _starMark.SpawnCosmicFieldLine(coords, DirectionFlag.East, -4, 4, 3, strength);
        }
    }

    private void OnStarBlast(EventHereticStarBlast args)
    {
        if (!TryComp(args.Action, out StarBlastActionComponent? starBlast))
            return;

        if (!TryUseAbility(args, false))
            return;

        var ent = args.Performer;

        Heretic.TryGetHereticComponent(ent, out var heretic, out _);
        var strength = heretic is {CurrentPath: "Cosmos"} ? heretic.PathStage : 10;

        if (Exists(starBlast.Projectile))
        {
            _actions.SetIfBiggerCooldown(args.Action!, starBlast.Cooldown);

            var newCoords = Transform(starBlast.Projectile).Coordinates;
            var oldCoords = Transform(ent).Coordinates;

            PredictedSpawnAtPosition(starBlast.Effect, oldCoords);
            _audio.PlayPredicted(starBlast.Sound, oldCoords, args.Performer);
            PullVictims(ent, oldCoords, strength);
            _pulling.StopAllPulls(ent);
            _transform.SetCoordinates(ent, newCoords);
            PredictedSpawnAtPosition(starBlast.Effect, newCoords);
            _audio.PlayPredicted(starBlast.Sound, newCoords, args.Performer);
            PullVictims(ent, newCoords, strength);

            PredictedQueueDel(starBlast.Projectile);

            starBlast.Projectile = EntityUid.Invalid;
            Dirty(args.Action, starBlast);

            // Don't do args.Handled = true because it resets cooldown

            if (_net.IsServer)
                RaiseNetworkEvent(new StopTargetingEvent(), args.Performer);

            return;
        }

        if (!args.Target.IsValid(EntityManager))
            return;

        args.Handled = true;

        starBlast.Projectile = ShootProjectileSpell(args.Performer,
            args.Target,
            args.Projectile,
            args.ProjectileSpeed,
            args.Entity);
        EnsureComp<CosmicTrailComponent>(starBlast.Projectile).Strength = strength;
        EnsureComp<StarBlastComponent>(starBlast.Projectile).Action = args.Action;
        Dirty(args.Action, starBlast);
    }


    private void OnEntityTerminating(Entity<StarBlastComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.Action == EntityUid.Invalid || TerminatingOrDeleted(ent.Comp.Action) ||
            !TryComp(ent.Comp.Action, out StarBlastActionComponent? action))
            return;

        action.Projectile = EntityUid.Invalid;
        Dirty(ent.Comp.Action, action);
    }

    private void OnHit(Entity<StarBlastComponent> ent, ref ProjectileHitEvent args)
    {
        var coords = Transform(ent).Coordinates;
        _starMark.ApplyStarMarkInRange(coords, args.Shooter, ent.Comp.StarMarkRadius);

        if (TryComp(args.Target, out StatusEffectsComponent? targetStatus))
            _stun.KnockdownOrStun(args.Target, ent.Comp.KnockdownTime, true, targetStatus);
    }

    private void PullVictims(EntityUid user, EntityCoordinates coords, int strength)
    {
        foreach (var mob in GetNearbyPeople(user, 2f, "Cosmos", coords))
        {
            if (_starMark.TryApplyStarMark(mob.AsNullable()))
                _throw.TryThrow(mob, coords);
        }
        _starMark.SpawnCosmicFields(coords, 1, strength);
    }

    private void OnStarTouch(EventHereticStarTouch args)
    {
        var touch = GetTouchSpell<EventHereticStarTouch, StarTouchComponent>(args.Performer, ref args);
        if (touch == null)
            return;

        EnsureComp<StarTouchComponent>(touch.Value).Action = args.Action.Owner;
    }

    private void OnCosmicRune(EventHereticCosmicRune args)
    {
        if (!TryComp(args.Action, out HereticCosmicRuneActionComponent? runeAction))
            return;

        if (!TryUseAbility(args, false))
            return;

        var coords = Transform(args.Performer).Coordinates.SnapToGrid(EntityManager, _mapMan);

        // No placing runes on top of runes
        if (Lookup.GetEntitiesInRange<HereticCosmicRuneComponent>(coords, 0.4f).Count > 0)
        {
            Popup.PopupClient(Loc.GetString("heretic-ability-fail-tile-occupied"), args.Performer, args.Performer);
            return;
        }

        args.Handled = true;

        if (_net.IsClient)
            return;

        var firstRuneResolved = Exists(runeAction.FirstRune);
        var secondRuneResolved = Exists(runeAction.SecondRune);

        if (firstRuneResolved && secondRuneResolved)
        {
            EnsureComp<FadingTimedDespawnComponent>(runeAction.FirstRune!.Value).Lifetime = 0f;
            var newRune = Spawn(args.Rune, coords);
            _transform.AttachToGridOrMap(newRune);
            var newRuneComp = EnsureComp<HereticCosmicRuneComponent>(newRune);
            var secondRuneComp = EnsureComp<HereticCosmicRuneComponent>(runeAction.SecondRune!.Value);
            newRuneComp.LinkedRune = runeAction.SecondRune.Value;
            secondRuneComp.LinkedRune = newRune;
            DirtyField(newRune, newRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            DirtyField(runeAction.SecondRune.Value, secondRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            runeAction.FirstRune = runeAction.SecondRune.Value;
            runeAction.SecondRune = newRune;
            return;
        }

        if (!firstRuneResolved)
        {
            var newRune = Spawn(args.Rune, coords);
            _transform.AttachToGridOrMap(newRune);
            runeAction.FirstRune = newRune;

            if (!secondRuneResolved)
                return;

            var newRuneComp = EnsureComp<HereticCosmicRuneComponent>(newRune);
            var secondRuneComp = EnsureComp<HereticCosmicRuneComponent>(runeAction.SecondRune!.Value);
            newRuneComp.LinkedRune = runeAction.SecondRune.Value;
            secondRuneComp.LinkedRune = newRune;
            DirtyField(newRune, newRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            DirtyField(runeAction.SecondRune.Value, secondRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            return;
        }


        if (!secondRuneResolved)
        {
            var newRune = Spawn(args.Rune, coords);
            _transform.AttachToGridOrMap(newRune);
            runeAction.SecondRune = newRune;

            if (!firstRuneResolved)
                return;

            var newRuneComp = EnsureComp<HereticCosmicRuneComponent>(newRune);
            var firstRuneComp = EnsureComp<HereticCosmicRuneComponent>(runeAction.FirstRune!.Value);
            newRuneComp.LinkedRune = runeAction.FirstRune.Value;
            firstRuneComp.LinkedRune = newRune;
            DirtyField(newRune, newRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
            DirtyField(runeAction.FirstRune.Value, firstRuneComp, nameof(HereticCosmicRuneComponent.LinkedRune));
        }
    }
}
