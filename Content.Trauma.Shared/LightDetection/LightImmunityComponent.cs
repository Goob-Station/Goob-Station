// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.LightDetection;

/// <summary>
/// Grants light immunity for X seconds.
/// Should be used for entities with Light Detection mechanics that need to survive the first seconds of spawning.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class LightImmunityComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextUpdate;
}
