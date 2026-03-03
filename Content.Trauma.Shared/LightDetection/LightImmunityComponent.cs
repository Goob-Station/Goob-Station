// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Trauma.Shared.LightDetection;

/// <summary>
/// Grants light immunity for X seconds.
/// Should be used for entities with Light Detection mechanics that need to survive the first seconds of spawning.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class LightImmunityComponent : Component
{
    /// <summary>
    /// How long the light immunity lasts
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate;
}
