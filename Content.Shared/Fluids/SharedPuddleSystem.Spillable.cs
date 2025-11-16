// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Doctor-Cpu <77215380+Doctor-Cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry; // Goobstation
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems; // Goobstation
using Content.Shared.Chemistry.Reaction; // Goobstation
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.IdentityManagement; // Goobstation
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups; // Goobstation
using Content.Shared.Spillable;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events; // Goobstation
using Robust.Shared.Network; // Gaby

namespace Content.Shared.Fluids;

public abstract partial class SharedPuddleSystem
{
    [Dependency] protected readonly OpenableSystem Openable = default!;
    [Dependency] private readonly INetManager _net = default!; // Gaby

    protected virtual void InitializeSpillable()
    {
        SubscribeLocalEvent<SpillableComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SpillableComponent, GetVerbsEvent<Verb>>(AddSpillVerb);
        SubscribeLocalEvent<SpillableComponent, MeleeHitEvent>(SplashOnMeleeHit, after: [typeof(OpenableSystem)]); // Goobstation
    }

    private void OnExamined(Entity<SpillableComponent> entity, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(SpillableComponent)))
        {
            args.PushMarkup(Loc.GetString("spill-examine-is-spillable"));

            if (HasComp<MeleeWeaponComponent>(entity))
                args.PushMarkup(Loc.GetString("spill-examine-spillable-weapon"));
        }
    }

    private void AddSpillVerb(Entity<SpillableComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (!_solutionContainerSystem.TryGetSolution(args.Target, entity.Comp.SolutionName, out var soln, out var solution))
            return;

        if (Openable.IsClosed(args.Target))
            return;

        if (solution.Volume == FixedPoint2.Zero)
            return;

        Verb verb = new()
        {
            Text = Loc.GetString("spill-target-verb-get-data-text")
        };

        // TODO VERB ICONS spill icon? pouring out a glass/beaker?
        if (entity.Comp.SpillDelay == null)
        {
            var target = args.Target;
            verb.Act = () =>
            {
                var puddleSolution = _solutionContainerSystem.SplitSolution(soln.Value, solution.Volume);
                TrySpillAt(Transform(target).Coordinates, puddleSolution, out _);

                if (TryComp<InjectorComponent>(entity, out var injectorComp))
                {
                    injectorComp.ToggleState = InjectorToggleMode.Draw;
                    Dirty(entity, injectorComp);
                }
            };
        }
        else
        {
            var user = args.User;
            verb.Act = () =>
            {
                _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, user, entity.Comp.SpillDelay ?? 0, new SpillDoAfterEvent(), entity.Owner, target: entity.Owner)
                {
                    BreakOnDamage = true,
                    BreakOnMove = true,
                    NeedHand = true,
                });
            };
        }
        verb.Impact = LogImpact.Medium; // dangerous reagent reaction are logged separately.
        verb.DoContactInteraction = true;
        args.Verbs.Add(verb);
    }

    private void SplashOnMeleeHit(Entity<SpillableComponent> entity, ref MeleeHitEvent args) // Goobstation, moved from server
    {
        if (args.Handled)
            return;

        if (!_net.IsServer)
            return;

        // When attacking someone reactive with a spillable entity,
        // splash a little on them (touch react)
        // If this also has solution transfer, then assume the transfer amount is how much we want to spill.
        // Otherwise let's say they want to spill a quarter of its max volume.

        if (!_solutionContainerSystem.TryGetDrainableSolution(entity.Owner, out var soln, out var solution))
            return;

        var totalSplit = FixedPoint2.Min(solution.MaxVolume * 0.25, solution.Volume);
        if (TryComp<SolutionTransferComponent>(entity, out var transfer))
        {
            totalSplit = FixedPoint2.Min(transfer.TransferAmount, solution.Volume);
        }

        // a little lame, but reagent quantity is not very balanced and we don't want people
        // spilling like 100u of reagent on someone at once!
        totalSplit = FixedPoint2.Min(totalSplit, entity.Comp.MaxMeleeSpillAmount);

        if (totalSplit <= 0)
            return;

        if (_net.IsServer)
        {
            args.Handled = true;
        }

        var reactiveTargets = 0;
        foreach (var hit in args.HitEntities)
        {
            if (HasComp<ReactiveComponent>(hit))
                reactiveTargets++;
        }

        if (reactiveTargets == 0)
            return;

        var amountPerTarget = totalSplit / reactiveTargets;

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<ReactiveComponent>(hit))
                continue;

            var splitSolution = _solutionContainerSystem.SplitSolution(soln.Value, amountPerTarget);
            if (splitSolution.Volume <= 0)
                continue;

            var ev = new SpilledOnEvent(entity.Owner, splitSolution.Clone());
            RaiseLocalEvent(hit, ev);

            if (_net.IsServer)
                AdminLogger.Add(LogType.MeleeHit, $"{ToPrettyString(args.User)} splashed {SharedSolutionContainerSystem.ToPrettyString(splitSolution):solution} from {ToPrettyString(entity.Owner):entity} onto {ToPrettyString(hit):target}");

            Reactive.DoEntityReaction(hit, splitSolution, ReactionMethod.Touch);

            Popups.PopupPredicted(Loc.GetString("spill-melee-hit-attacker", ("amount", amountPerTarget), ("spillable", entity.Owner), ("target", Identity.Entity(hit, EntityManager))),
                Loc.GetString("spill-melee-hit-others", ("attacker", args.User), ("spillable", entity.Owner), ("target", Identity.Entity(hit, EntityManager))),
                hit, args.User, PopupType.SmallCaution);
        }
    }
}
