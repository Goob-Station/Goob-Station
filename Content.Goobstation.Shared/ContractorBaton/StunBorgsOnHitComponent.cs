// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.ContractorBaton;

[RegisterComponent]
public sealed partial class StunBorgsOnHitComponent : Component
{
    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);
}