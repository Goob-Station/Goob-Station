// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// Indicates this entity is currently held inside of a station AI core.
/// </summary>
[RegisterComponent, NetworkedComponent]
// Corvax-Next-AiRemoteControl-Start
public sealed partial class StationAiHeldComponent : Component
{
    [DataField]
    public EntityUid? CurrentConnectedEntity;
}
// Corvax-Next-AiRemoteControl-End
