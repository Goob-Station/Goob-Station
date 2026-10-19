// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// Attached to the roving AI eye (StationAiHolo) to track movement speed scaling with zoom.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StationAiEyeComponent : Component
{
    /// <summary>
    /// Current speed multiplier based on how far the AI has zoomed out.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SpeedModifier = 1.0f;
}
