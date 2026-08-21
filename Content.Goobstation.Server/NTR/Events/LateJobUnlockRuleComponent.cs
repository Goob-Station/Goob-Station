// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.NTR.Events;

[RegisterComponent]
public sealed partial class LateJobUnlockRuleComponent : Component
{
    /// <summary>
    /// Jobs to add slots for (jobId, slotCount)
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<JobPrototype>, int> JobsToAdd = new();
}
