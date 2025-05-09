// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Shared.Enchanting.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Goidastation.Shared.Enchanting.Components;

/// <summary>
/// Added to items after being enchanted.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(EnchantingSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class EnchantedComponent : Component
{
    /// <summary>
    /// The number of enchants this item can support.
    /// Can be increased by killing player-controlled mobs on an altar with this on the same tile.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Tier = 1;

    /// <summary>
    /// Players can be sacrificed up to this tier.
    /// </summary>
    [DataField]
    public int MaxTier = 3;

    /// <summary>
    /// The ID of the container to add.
    /// </summary>
    [DataField]
    public string ContainerId = "_goida_enchants";

    /// <summary>
    /// The container that stores enchant entities.
    /// </summary>
    [ViewVariables]
    public Container Container = default!;

    /// <summary>
    /// The enchants this item has.
    /// </summary>
    [ViewVariables]
    public IReadOnlyList<EntityUid> Enchants => Container.ContainedEntities;
}
