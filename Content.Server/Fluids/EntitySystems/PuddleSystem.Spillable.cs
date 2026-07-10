// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Solutions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry;
using Content.Shared.Database;
using Content.Shared._Pirate.Fluids; // Pirate: stains
using Content.Shared.Audio; // Pirate: stains
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Spillable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio; // Pirate: stains
using Robust.Shared.Player;

namespace Content.Server.Fluids.EntitySystems;

public sealed partial class PuddleSystem
{
    #region Pirate: stains
    private static readonly SoundSpecifier MeleeSplashSound = new SoundPathSpecifier(
        "/Audio/_Pirate/Effects/Fluids/slosh.ogg",
        AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(-3f));
    #endregion Pirate: stains

    protected override void InitializeSpillable()
    {
        base.InitializeSpillable();

        SubscribeLocalEvent<SpillableComponent, LandEvent>(SpillOnLand);
        SubscribeLocalEvent<SpillableComponent, SolutionContainerOverflowEvent>(OnOverflow);
        SubscribeLocalEvent<SpillableComponent, SpillDoAfterEvent>(OnDoAfter);
    }

    private void OnOverflow(Entity<SpillableComponent> entity, ref SolutionContainerOverflowEvent args)
    {
        if (args.Handled)
            return;

        TrySpillAt(Transform(entity).Coordinates, args.Overflow, out _);
        args.Handled = true;
    }

    protected override void SplashOnMeleeHit(Entity<SpillableComponent> entity, ref MeleeHitEvent args)
    {
        if (args.Handled)
            return;

        // When attacking someone reactive with a spillable entity,
        // splash a little on them (touch react)
        // If this also has solution transfer, then assume the transfer amount is how much we want to spill.
        // Otherwise let's say they want to spill a quarter of its max volume.

        if (!_solutionContainerSystem.TryGetDrainableSolution(entity.Owner, out var soln, out var solution))
            return;

        var hitCount = args.HitEntities.Count;

        var totalSplit = FixedPoint2.Min(solution.MaxVolume * 0.25, solution.Volume);
        if (TryComp<SolutionTransferComponent>(entity, out var transfer))
        {
            totalSplit = FixedPoint2.Min(transfer.TransferAmount, solution.Volume);
        }

        // a little lame, but reagent quantity is not very balanced and we don't want people
        // spilling like 100u of reagent on someone at once!
        totalSplit = FixedPoint2.Min(totalSplit, entity.Comp.MaxMeleeSpillAmount);

        if (totalSplit == 0)
            return;

        // Optionally allow further melee handling occur
        args.Handled = entity.Comp.PreventMelee;

        if (hitCount == 0 && args.Direction == null)
        {
            var puddleEv = new SpillableCreatePuddleOnHitEvent(args.User, args.Coords, totalSplit.Float());
            RaiseLocalEvent(entity, ref puddleEv);
            return;
        }

        // First update the hit count so anything that is not reactive wont count towards the total!
        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<ReactiveComponent>(hit))
                hitCount -= 1;
        }

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<ReactiveComponent>(hit))
                continue;

            var splitSolution = _solutionContainerSystem.SplitSolution(soln.Value, totalSplit / hitCount);

            AdminLogger.Add(LogType.MeleeHit, $"{ToPrettyString(args.User)} splashed {SharedSolutionContainerSystem.ToPrettyString(splitSolution):solution} from {ToPrettyString(entity.Owner):entity} onto {ToPrettyString(hit):target}");
            RaiseLocalEvent(hit, new SpilledOnEvent(entity.Owner, splitSolution.Clone())); // Pirate: stains
            PlayMeleeSplashEffect(hit, splitSolution); // Pirate: stains
            Reactive.DoEntityReaction(hit, splitSolution, ReactionMethod.Touch);

            Popups.PopupEntity(
                Loc.GetString("spill-melee-hit-attacker", ("amount", totalSplit / hitCount), ("spillable", entity.Owner),
                    ("target", Identity.Entity(hit, EntityManager))),
                hit, args.User);

            Popups.PopupEntity(
                Loc.GetString("spill-melee-hit-others", ("attacker", Identity.Name(args.User, EntityManager)), ("spillable", entity.Owner), // Goobstation - indentity hidden on splash
                    ("target", Identity.Entity(hit, EntityManager))),
                hit, Filter.PvsExcept(args.User), true, PopupType.SmallCaution);
        }
    }

    #region Pirate: stains
    private void PlayMeleeSplashEffect(EntityUid target, Solution solution)
    {
        Audio.PlayPvs(MeleeSplashSound, target);
        RaiseNetworkEvent(new LiquidSplashEffectEvent(GetNetEntity(target), solution.GetColor(_prototypeManager)),
            Filter.Pvs(target, entityManager: EntityManager));
    }
    #endregion Pirate: stains

    private void SpillOnLand(Entity<SpillableComponent> entity, ref LandEvent args)
    {
        if (!entity.Comp.SpillWhenThrown || Openable.IsClosed(entity.Owner))
            return;

        if (TrySplashSpillAt(entity.Owner, Transform(entity).Coordinates, out _, out var solution) && args.User != null)
        {
            AdminLogger.Add(LogType.Landed,
                $"{ToPrettyString(entity.Owner):entity} spilled a solution {SharedSolutionContainerSystem.ToPrettyString(solution):solution} on landing");
        }
    }

    private void OnDoAfter(Entity<SpillableComponent> entity, ref SpillDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target == null)
            return;

        //solution gone by other means before doafter completes
        if (!_solutionContainerSystem.TryGetDrainableSolution(entity.Owner, out var soln, out var solution) || solution.Volume == 0)
            return;

        var puddleSolution = _solutionContainerSystem.SplitSolution(soln.Value, solution.Volume);
        TrySpillAt(Transform(entity).Coordinates, puddleSolution, out _);
        args.Handled = true;
    }
}
