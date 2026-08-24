// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Overlays;

/// <summary>
/// This component allows you to see health status icons above damageable mobs.
/// </summary>
[RegisterComponent, NetworkedComponent,
 AutoGenerateComponentState(raiseAfterAutoHandleState: true)] // Shitmed Change
public sealed partial class ShowHealthIconsComponent : Component
{
    // Goobstation
    [DataField]
    public bool WorksInHands;

    /// <summary>
    /// Displays health status icons of the damage containers.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public List<ProtoId<DamageContainerPrototype>> DamageContainers = new()
    {
        "Biological"
    };
}
