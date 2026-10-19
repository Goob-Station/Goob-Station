// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// Indicates this entity is currently held inside of a station AI core.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
// Corvax-Next-AiRemoteControl-Start
public sealed partial class StationAiHeldComponent : Component
{
    [DataField]
    public EntityUid? CurrentConnectedEntity;

    /// <summary>
    /// Whether the AI's eye is visible on crew monitoring systems.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool EyeVisibleOnCrewMonitor = true;
}
// Corvax-Next-AiRemoteControl-End
