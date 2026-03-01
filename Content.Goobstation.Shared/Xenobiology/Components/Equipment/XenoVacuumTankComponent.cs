// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Xenobiology.Components.Equipment;

/// <summary>
/// This handles the tanks for xeno vacuums.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenoVacuumTankComponent : Component
{
    [DataField]
    public string TankContainerName = "StorageTank";

    /// <summary>
    /// The ID of the tank's container.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Container StorageTank = new();

    /// <summary>
    /// The maximum amount of entities in this tank at a time.
    /// Will be upgradable.
    /// </summary>
    [DataField]
    public int MaxEntities = 5;

    /// <summary>
    /// The EntityUid of the nozzle attached to this tank.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? LinkedNozzle;
}
