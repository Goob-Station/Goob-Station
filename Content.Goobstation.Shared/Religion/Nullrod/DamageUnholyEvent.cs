// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Religion.Nullrod;

[ByRefEvent]
public sealed class DamageUnholyEvent(EntityUid target, EntityUid? origin = null) : EntityEventArgs
{
    public readonly EntityUid Target = target;

    public bool ShouldTakeHoly = false;

    public EntityUid? Origin = origin;
}
