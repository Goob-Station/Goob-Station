// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Common.Pirates;

[RegisterComponent]
public sealed partial class ResourceSiphonComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)] public EntityUid? BoundGamerule;
    [ViewVariables(VVAccess.ReadOnly)] public bool Active = false;

    [DataField] public float CreditsThreshold = 100000f;

    [ViewVariables(VVAccess.ReadWrite)] public float Credits = 0f;

    [DataField] public float DrainRate = 10f;

    [ViewVariables(VVAccess.ReadOnly)] public ResourceSiphonActivationPhase ActivationPhase = ResourceSiphonActivationPhase.Idle;
    [ViewVariables(VVAccess.ReadOnly)] public float ActivationRewindTime = 3.5f;
    [ViewVariables(VVAccess.ReadOnly)] public float ActivationRewindClock = 3.5f;

    [DataField] public float MaxSignalRange = 100f;
}

public enum ResourceSiphonActivationPhase
{
    Idle = 0,
    FirstWarning = 1,
    SecondWarning = 2,
    Armed = 3,
}
