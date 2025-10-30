using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Common.Temperature.Components;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Hands.Components;
using Content.Shared.Heretic;
using Content.Shared.Projectiles;
using Content.Shared.StatusEffect;
using Robust.Shared.Map;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeCosmos()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticCosmicRune>(OnCosmicRune);
        SubscribeLocalEvent<HereticComponent, EventHereticStarTouch>(OnStarTouch);
        SubscribeLocalEvent<HereticComponent, EventHereticStarBlast>(OnHereticStarBlast);
        SubscribeLocalEvent<HereticComponent, EventHereticCosmicExpansion>(OnHereticExpansion);
        SubscribeLocalEvent<HereticComponent, EventHereticCosmosPassive>(OnPassive);
        SubscribeLocalEvent<HereticComponent, HereticAscensionCosmosEvent>(OnAscension);

        SubscribeLocalEvent<StarGazerComponent, EventHereticStarBlast>(OnStarGazerStarBlast);
        SubscribeLocalEvent<StarGazerComponent, EventHereticCosmicExpansion>(OnStarGazerExpansion);

        SubscribeLocalEvent<StarBlastComponent, ProjectileHitEvent>(OnHit);
        SubscribeLocalEvent<StarBlastComponent, EntityTerminatingEvent>(OnEntityTerminating);
    }

    protected virtual void OnAscension(Entity<HereticComponent> ent, ref HereticAscensionCosmosEvent args)
    {
        EnsureComp<SpecialHighTempImmunityComponent>(ent);
        EnsureComp<SpecialLowTempImmunityComponent>(ent);
        EnsureComp<SpecialPressureImmunityComponent>(ent);

        _eye.SetDrawFov(ent, false);
    }

    private void OnStarGazerExpansion(Entity<StarGazerComponent> ent, ref EventHereticCosmicExpansion args)
    {
        OnExpansion(ent, ref args, 10, true);
    }

    private void OnStarGazerStarBlast(Entity<StarGazerComponent> ent, ref EventHereticStarBlast args)
    {
        OnStarBlast(ent, ref args, 10);
    }

    private void OnHereticExpansion(Entity<HereticComponent> ent, ref EventHereticCosmicExpansion args)
    {
        OnExpansion(ent, ref args, ent.Comp.PathStage, ent.Comp is { Ascended: true, CurrentPath: "Cosmos" });
    }

    private void OnHereticStarBlast(Entity<HereticComponent> ent, ref EventHereticStarBlast args)
    {
        OnStarBlast(ent, ref args, ent.Comp.PathStage);
    }

    private void OnPassive(Entity<HereticComponent> ent, ref EventHereticCosmosPassive args)
    {
        EnsureComp<CosmosPassiveComponent>(ent);
    }

    private void OnExpansion(EntityUid ent, ref EventHereticCosmicExpansion args, int strength, bool ascended)
    {
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        var coords = Transform(ent).Coordinates;

        _starMark.ApplyStarMarkInRange(coords, ent, args.Range);
        _starMark.SpawnCosmicFields(coords, 2, strength);

        PredictedSpawnAtPosition(args.Effect, coords);

        if (!ascended)
            return;

        _starMark.SpawnCosmicFieldLine(coords, DirectionFlag.North, -4, 4, 3, strength);
        _starMark.SpawnCosmicFieldLine(coords, DirectionFlag.East, -4, 4, 3, strength);
    }

    private void OnStarBlast(EntityUid ent, ref EventHereticStarBlast args, int strength)
    {
        if (!TryComp(args.Action, out StarBlastActionComponent? starBlast))
            return;

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

        if (!TryUseAbility(ent, args))
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

    private void OnStarTouch(Entity<HereticComponent> ent, ref EventHereticStarTouch args)
    {
        var touch = GetTouchSpell<EventHereticStarTouch, StarTouchComponent>(ent, ref args);
        if (touch == null)
            return;

        EnsureComp<StarTouchComponent>(touch.Value).Action = args.Action.Owner;
    }

    private void OnCosmicRune(Entity<HereticComponent> ent, ref EventHereticCosmicRune args)
    {
        if (!TryComp(args.Action, out HereticCosmicRuneActionComponent? runeAction))
            return;

        var coords = Transform(ent).Coordinates.SnapToGrid(EntityManager, _mapMan);

        // No placing runes on top of runes
        if (Lookup.GetEntitiesInRange<HereticCosmicRuneComponent>(coords, 0.4f).Count > 0)
        {
            Popup.PopupClient(Loc.GetString("heretic-ability-fail-tile-occupied"), args.Performer, args.Performer);
            return;
        }

        if (!TryUseAbility(ent, args))
            return;

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
