// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SilverMaelstromComponent : Component
{
    public override bool SessionSpecific => true;

    [DataField] public float RespawnCooldown = 7.5f;
    [ViewVariables(VVAccess.ReadWrite)] public float RespawnTimer = 0f;

    [ViewVariables(VVAccess.ReadOnly)] public int ActiveBlades = 0;
    [DataField] public int MaxBlades = 5;
}
