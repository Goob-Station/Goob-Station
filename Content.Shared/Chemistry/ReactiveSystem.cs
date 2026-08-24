// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Chemistry;
using Content.Goobstation.Common.Slasher.Events;
using Content.Shared.Administration.Logs;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Shared.Chemistry;

[UsedImplicitly]
public sealed class ReactiveSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public void DoEntityReaction(EntityUid uid, Solution solution, ReactionMethod method)
    {
        foreach (var reagent in solution.Contents.ToArray())
        {
            ReactionEntity(uid, method, reagent);
        }
    }

    public void ReactionEntity(EntityUid uid, ReactionMethod method, ReagentQuantity reagentQuantity)
    {
        if (reagentQuantity.Quantity == FixedPoint2.Zero)
            return;

        // We throw if the reagent specified doesn't exist.
        if (!_proto.Resolve<ReagentPrototype>(reagentQuantity.Reagent.Prototype, out var proto))
            return;

        var ev = new ReactionEntityEvent(method, reagentQuantity, proto);
        RaiseLocalEvent(uid, ref ev);

        if (method == ReactionMethod.Touch) // goob kill me
        {
            // This might be the most horrendous shit i've done. I need you to ping me if you are reading this because i should've fixed it before anyone could see.
            // todo marty
            var relayEv = new ShitRelayEventFixMeReactionEntityEvent();
            RaiseLocalEvent(uid, ref relayEv);
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
    ReagentQuantity ReagentQuantity,
    ReagentPrototype Reagent);
