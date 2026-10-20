// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marks a material silo as a Malfunctioning Silo deployed by a Malf AI.
/// All factory machines and ore silo clients automatically link to this silo.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MalfAiSiloComponent : Component
{
    /// <summary>
    /// The Malfunctioning AI that deployed this silo.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Controller;
}
