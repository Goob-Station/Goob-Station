// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Weapons.DelayedKnockdown;

[RegisterComponent]
public sealed partial class ModifyDelayedKnockdownComponent : Component
{
    [DataField]
    public bool Cancel;

    [DataField]
    public float DelayDelta;

    [DataField]
    public float KnockdownTimeDelta;
}
