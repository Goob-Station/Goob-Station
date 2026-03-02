// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.ShadowDemon.ShadowCocoon;

/// <summary>
/// Add this to an entity to give them the ability to put a humanoid entity into a shadow cocoon
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowCocoonMakerComponent : Component
{
    /// <summary>
    /// The shadow cocoon entity
    /// </summary>
    [DataField]
    public EntProtoId ShadowCocoon = "ShadowCocoon";

    /// <summary>
    /// The doafter delay when cocooning someone
    /// </summary>
    [DataField]
    public TimeSpan CocoonDelay = TimeSpan.FromSeconds(4);
}
