// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Projectiles;

public sealed class ShouldTargetedProjectileCollideEvent(EntityUid target) : HandledEntityEventArgs
{
    public EntityUid Target = target;
}
