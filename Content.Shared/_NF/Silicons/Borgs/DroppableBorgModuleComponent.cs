// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._NF.Silicons.Borgs;

/// <summary>
/// Uses placeholder entities to give borgs "hands" that are whitelisted for a certain kind of item.
/// The items in it can be dropped and picked up if they match its whitelist.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(DroppableBorgModuleSystem))]
public sealed partial class DroppableBorgModuleComponent : Component
{
    /// <summary>
    /// The items to spawn in borg hands.
    /// </summary>
    [DataField(required: true)]
    public List<DroppableBorgItem> Items = new();

    /// <summary>
    /// The ID of the container to add that stores items when not in hands.
    /// </summary>
    [DataField]
    public string ContainerId = "nf-droppable-items";

    /// <summary>
    /// The ID of the container to add that stores item placeholders when not in hands.
    /// </summary>
    [DataField]
    public string PlaceholderContainerId = "nf-item-placeholders";

    /// <summary>
    /// The ID to check for module equivalence.
    /// </summary>
    [DataField(required: true)]
    public string ModuleId = default!;
}

[DataDefinition]
public sealed partial class DroppableBorgItem
{
    /// <summary>
    /// The entity to spawn and use for the placeholder sprite.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Id;

    /// <summary>
    /// A whitelist that items must match to be picked up by the placeholder.
    /// Regardless of this whitelist entities must have <c>ItemComponent</c> to be picked up.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist;
}