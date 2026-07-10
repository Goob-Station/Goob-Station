// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.BloodCult;
using Content.Shared.Fluids;

namespace Content.Server.Fluids.EntitySystems;

/// <inheritdoc/>
public sealed class AbsorbentSystem : SharedAbsorbentSystem
{
    protected override bool TryBeforeMopTarget(Entity<AbsorbentComponent> absorbEnt, EntityUid user, EntityUid target)
    {
        // Pirate: let Blood Cult runes consume mop interactions before puddle/refillable handling.
        var ev = new AbsorbentMopTargetEvent(user, target, absorbEnt.Owner, absorbEnt.Comp);
        RaiseLocalEvent(target, ref ev);
        return ev.Handled;
    }
}
