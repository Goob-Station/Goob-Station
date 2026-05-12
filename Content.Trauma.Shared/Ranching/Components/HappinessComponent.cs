// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.InternalResources.Data;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// Used for happiness for ranching does not actually store the happiness, it uses the internal resources system for that
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HappinessComponent : Component
{
    [DataField]
    public ProtoId<InternalResourcesPrototype> HappinessResource = "Happiness";

    [DataField]
    public float HappinessIncrease = 1f;

    [DataField]
    public float DamageDecrease = -10f;
}
