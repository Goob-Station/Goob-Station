// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// This handles the tanks for xeno vacuums.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenoVacuumTankComponent : Component
{
    /// <summary>
    /// The ID of the tank's container.
    /// </summary>
    [DataField]
    public Container StorageTank = new();

    /// <summary>
    /// The EntityUid of the nozzle attached to this tank.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedNozzle;
}
