// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Chemistry;
using Content.Shared.Administration.Logs;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared.Chemistry;

[UsedImplicitly]
public sealed class ReactiveSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedEntityEffectSystem _effects = default!; // goob edit

    public void DoEntityReaction(EntityUid uid, Solution solution, ReactionMethod method)
    {
        foreach (var reagent in solution.Contents.ToArray())
        {
            ReactionEntity(uid, method, reagent, solution);
        }
    }

    public void ReactionEntity(EntityUid uid, ReactionMethod method, ReagentQuantity reagentQuantity, Solution? source)
    {
        // We throw if the reagent specified doesn't exist.
        var proto = _prototypeManager.Index<ReagentPrototype>(reagentQuantity.Reagent.Prototype);
        ReactionEntity(uid, method, proto, reagentQuantity, source);
    }

    public void ReactionEntity(EntityUid uid, ReactionMethod method, ReagentPrototype proto,
        ReagentQuantity reagentQuantity, Solution? source)
    {
        if (!TryComp<ReactiveComponent>(uid, out var reactive))
            return;

        // custom event for bypassing reactivecomponent stuff
        var ev = new ReactionEntityEvent(method, proto, reagentQuantity, source);
        RaiseLocalEvent(uid, ref ev);

        // If we have a source solution, use the reagent quantity we have left. Otherwise, use the reaction volume specified.
        var args = new EntityEffectReagentArgs(uid, EntityManager, null, source, source?.GetReagentQuantity(reagentQuantity.Reagent) ?? reagentQuantity.Quantity, proto, method, 1f);

        if (reactive.OneUnitReaction) // goob edit
            args.Quantity = 1;

        // First, check if the reagent wants to apply any effects.
        if (proto.ReactiveEffects != null && reactive.ReactiveGroups != null)
        {
            foreach (var (key, val) in proto.ReactiveEffects)
            {
                if (!val.Methods.Contains(method))
                    continue;

                if (!reactive.ReactiveGroups.ContainsKey(key))
                    continue;

                if (!reactive.ReactiveGroups[key].Contains(method))
                    continue;

                // Goobstation - Start

                var beforeReact = new BeforeSolutionReactEvent();
                RaiseLocalEvent(uid, ref beforeReact);

                if (beforeReact.Cancelled)
                    continue;

                // Goobstation - End

                foreach (var effect in val.Effects)
                {
                    if (!effect.ShouldApply(args, _robustRandom))
                        continue;

                    if (effect.ShouldLog)
                    {
                        var entity = args.TargetEntity;
                        _adminLogger.Add(LogType.ReagentEffect, effect.LogImpact,
                            $"Reactive effect {effect.GetType().Name:effect} of reagent {proto.ID:reagent} with method {method} applied on entity {ToPrettyString(entity):entity} at {Transform(entity).Coordinates:coordinates}");
                    }

                    _effects.Effect(effect, args); // goob edit - use system instead
                }
            }
        }

        // Then, check if the prototype has any effects it can apply as well.
        if (reactive.Reactions != null)
        {
            foreach (var entry in reactive.Reactions)
            {
                if (!entry.Methods.Contains(method))
                    continue;

                if (entry.Reagents != null && !entry.Reagents.Contains(proto.ID))
                    continue;

                foreach (var effect in entry.Effects)
                {
                    if (!effect.ShouldApply(args, _robustRandom))
                        continue;

                    if (effect.ShouldLog)
                    {
                        var entity = args.TargetEntity;
                        _adminLogger.Add(LogType.ReagentEffect, effect.LogImpact,
                            $"Reactive effect {effect.GetType().Name:effect} of {ToPrettyString(entity):entity} using reagent {proto.ID:reagent} with method {method} at {Transform(entity).Coordinates:coordinates}");
                    }

                    _effects.Effect(effect, args); // goob edit - use system instead
                }
            }
        }
    }
}
public enum ReactionMethod
{
    Touch,
    Injection,
    Ingestion,
    Eyes,
}

[ByRefEvent]
public readonly record struct ReactionEntityEvent(
    ReactionMethod Method,
    ReagentPrototype Reagent,
    ReagentQuantity ReagentQuantity,
    Solution? Source
);
