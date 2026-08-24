// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._EinsteinEngines.Silicon.Death;

// Goobstation - Moved into shared, made networked.
/// <summary>
///     Marks a Silicon as becoming incapacitated when they run out of battery charge.
/// </summary>
/// <remarks>
///     Uses the Silicon System's charge states to do so, so make sure they're a battery powered Silicon.
/// </remarks>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SiliconDownOnDeadComponent : Component
{
    /// <summary>
    ///     Is this Silicon currently dead?
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public bool Dead;

    // Goobstation - Energycrit
    /// <summary>
    ///     Should we restore the ComplexInteractionComponent when we become powered again.
    /// </summary>
    [AutoNetworkedField]
    public bool CanUseComplexInteractions;
}
