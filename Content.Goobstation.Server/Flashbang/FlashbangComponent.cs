// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Flashbang;

[RegisterComponent]
public sealed partial class FlashbangComponent : Component
{
    [DataField]
    public float StunTime = 2f;

    [DataField]
    public float KnockdownTime = 10f;

    /// <summary>
    /// Minimum protection range on entity for stun and knocked down effects to be applied
    /// </summary>
    [DataField]
    public float MinProtectionRange;
}
